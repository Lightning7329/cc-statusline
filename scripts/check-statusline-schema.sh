#!/bin/bash
# 公式ドキュメントの statusline JSON スキーマ（Full JSON schema の例）からキーパス一覧を抽出し、
# schema/statusline-keys.txt（既知のキー一覧）と比較する。
#
# 差分があれば次を行う。
#   - スナップショットを最新のキー一覧で書き換える（先頭コメントに検出日と Claude Code の最新版を記録）
#   - PR 本文用の Markdown を $BODY_FILE（未指定なら stdout）に書き出す
#   - GitHub Actions 上では $GITHUB_OUTPUT に changed / version を書き出す
# 差分がなければ何も書き換えずに終了する。
# ドキュメントの構造変更などで抽出に失敗した場合は非 0 で終了する。

set -euo pipefail

readonly DOCS_URL="https://code.claude.com/docs/en/statusline.md"
readonly NPM_URL="https://registry.npmjs.org/@anthropic-ai/claude-code/latest"

repo_root=$(cd "$(dirname "$0")/.." && pwd)
snapshot="${SNAPSHOT:-$repo_root/schema/statusline-keys.txt}"
body_file="${BODY_FILE:-/dev/stdout}"

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT

# DOCS_FILE を指定すると、取得の代わりにローカルの Markdown を使う（スクリプト自体の動作確認用）
if [[ -n "${DOCS_FILE:-}" ]]; then
  cp "$DOCS_FILE" "$work/docs.md"
else
  curl -fsSL "$DOCS_URL" -o "$work/docs.md"
fi

# 「Full JSON schema」の見出し直後にある ```json ブロックを取り出す。
# ブロックは引用（行頭の "> "）の中にあるので、その接頭辞を外す。
awk '
  /Full JSON schema/        { found = 1; next }
  found && !inblock && /^> *```json/ { inblock = 1; next }
  inblock && /^> *```/      { closed = 1; exit }
  inblock                   { sub(/^> ?/, ""); print }
  END                       { if (!closed) exit 1 }
' "$work/docs.md" >"$work/schema.json" || {
  echo "error: Full JSON schema の json ブロックが見つかりません（ドキュメントの構造が変わった可能性があります）" >&2
  exit 1
}

# 中間ノードも含めた全キーパスを列挙する。配列の添字は [] で表す（例: foo[].bar）。
if ! jq -r '
  paths
  | map(if type == "number" then "[]" else . end)
  | reduce .[] as $p (""; if $p == "[]" then . + "[]" elif . == "" then $p else . + "." + $p end)
' "$work/schema.json" | LC_ALL=C sort -u >"$work/current.txt"; then
  echo "error: Full JSON schema のブロックが JSON として解釈できません" >&2
  exit 1
fi

if [[ ! -s "$work/current.txt" ]]; then
  echo "error: キーが 1 件も抽出できませんでした" >&2
  exit 1
fi

# スナップショットのコメント行（# 始まり）と空行を除いたものが既知のキー一覧
if [[ -f "$snapshot" ]]; then
  grep -v -e '^#' -e '^$' "$snapshot" | LC_ALL=C sort -u >"$work/known.txt" || true
else
  : >"$work/known.txt"
fi

LC_ALL=C comm -13 "$work/known.txt" "$work/current.txt" >"$work/added.txt"
LC_ALL=C comm -23 "$work/known.txt" "$work/current.txt" >"$work/removed.txt"

if [[ ! -s "$work/added.txt" && ! -s "$work/removed.txt" ]]; then
  echo "差分はありません" >&2
  [[ -n "${GITHUB_OUTPUT:-}" ]] && echo "changed=false" >>"$GITHUB_OUTPUT"
  exit 0
fi

version=$(curl -fsSL "$NPM_URL" | jq -r '.version')
detected=$(date -u +%F)

mkdir -p "$(dirname "$snapshot")"
{
  echo "# Source: $DOCS_URL"
  echo "# Detected: $detected (Claude Code latest on npm: $version)"
  cat "$work/current.txt"
} >"$snapshot"

# キー一覧を Markdown の箇条書きにする
bullets() {
  if [[ -s "$1" ]]; then
    # shellcheck disable=SC2016 # バッククォートは Markdown のコード表記で、展開させない
    sed 's/.*/- `&`/' "$1"
  else
    echo "なし"
  fi
}

# 追加キーに対応するフィールド一覧表の行（1 列目にキー名がそのまま書かれている行）を抜き出す
table_rows() {
  local key
  while IFS= read -r key; do
    grep -F -- "\`$key\`" "$work/docs.md" | grep '^| `' || true
  done <"$work/added.txt" | awk '!seen[$0]++' |
    # ページ内アンカーは PR 上では切れるので、ドキュメントの URL に置き換える
    sed 's|](#|](https://code.claude.com/docs/en/statusline#|g'
}

rows=$(table_rows)

{
  echo "公式ドキュメントの statusline JSON スキーマに、既知のキー一覧との差分を検出しました。"
  echo
  echo "- 取得元: $DOCS_URL"
  echo "- 検出日: $detected"
  echo "- 検出時点の Claude Code 最新版（npm）: \`$version\`"
  echo
  echo "## 追加されたキー"
  echo
  bullets "$work/added.txt"
  echo
  echo "## 削除されたキー"
  echo
  bullets "$work/removed.txt"
  echo
  echo "## ドキュメントのフィールド一覧表の該当行"
  echo
  if [[ -n "$rows" ]]; then
    echo "| Field | Description |"
    echo "| :- | :- |"
    echo "$rows"
  else
    echo "該当行なし（中間ノードのみの追加や、サブテーブルに記載されたフィールドは表に直接載らないことがあります）"
  fi
  echo
  echo "このPRをマージすると、上記の差分が確認済みとして \`schema/statusline-keys.txt\` に取り込まれます。対応するかどうかは個別に Issue 化して判断してください。"
} >"$body_file"

if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
  echo "changed=true" >>"$GITHUB_OUTPUT"
  echo "version=$version" >>"$GITHUB_OUTPUT"
fi
