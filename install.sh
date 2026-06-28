#!/bin/sh
set -eu

REPO="Lightning7329/cc-statusline"
BINARY_NAME="statusline"
DEFAULT_INSTALL_DIR="${HOME}/.claude/bin"

VERSION=""
INSTALL_DIR="${DEFAULT_INSTALL_DIR}"
SCOPE="user"

while [ $# -gt 0 ]; do
    case "$1" in
        --dir) INSTALL_DIR="$2"; shift 2 ;;
        --version) VERSION="$2"; shift 2 ;;
        --scope) SCOPE="$2"; shift 2 ;;
        *) printf "Unknown option: %s\n" "$1" >&2; exit 1 ;;
    esac
done

case "$SCOPE" in
    user) SETTINGS_FILE="${HOME}/.claude/settings.json" ;;
    project) SETTINGS_FILE=".claude/settings.json" ;;
    local) SETTINGS_FILE=".claude/settings.local.json" ;;
    *) printf "Error: --scope must be user, project, or local\n" >&2; exit 1 ;;
esac

if command -v curl >/dev/null 2>&1; then
    fetch() { curl -fsSL "$1"; }
    download() { curl -fsSL -o "$1" "$2"; }
elif command -v wget >/dev/null 2>&1; then
    fetch() { wget -qO- "$1"; }
    download() { wget -qO "$1" "$2"; }
else
    printf "Error: curl or wget is required\n" >&2
    exit 1
fi

OS="$(uname -s)"
case "$OS" in
    Linux*)  OS_NAME="linux" ;;
    Darwin*) OS_NAME="osx" ;;
    *) printf "Error: unsupported OS: %s\n" "$OS" >&2; exit 1 ;;
esac

ARCH="$(uname -m)"
case "$ARCH" in
    x86_64|amd64) ARCH_NAME="x64" ;;
    aarch64|arm64) ARCH_NAME="arm64" ;;
    *) printf "Error: unsupported architecture: %s\n" "$ARCH" >&2; exit 1 ;;
esac

if [ -z "$VERSION" ]; then
    VERSION="$(fetch "https://api.github.com/repos/${REPO}/releases/latest" \
        | grep '"tag_name"' | head -1 \
        | sed 's/.*"tag_name": *"//;s/".*//')"
    if [ -z "$VERSION" ]; then
        printf "Error: could not determine latest version\n" >&2
        exit 1
    fi
fi

case "$VERSION" in
    v*) TAG="$VERSION" ;;
    *) TAG="v${VERSION}" ;;
esac

ARCHIVE="statusline-${OS_NAME}-${ARCH_NAME}.tar.gz"
URL="https://github.com/${REPO}/releases/download/${TAG}/${ARCHIVE}"

printf "Downloading %s %s (%s-%s)...\n" "$BINARY_NAME" "$TAG" "$OS_NAME" "$ARCH_NAME"

TMPDIR_PATH="$(mktemp -d)"
trap 'rm -rf "$TMPDIR_PATH"' EXIT

download "${TMPDIR_PATH}/${ARCHIVE}" "$URL"

tar -xzf "${TMPDIR_PATH}/${ARCHIVE}" -C "$TMPDIR_PATH"

mkdir -p "$INSTALL_DIR"
INSTALL_DIR="$(cd "$INSTALL_DIR" && pwd)"

if [ -f "${INSTALL_DIR}/${BINARY_NAME}" ]; then
    printf "Replacing existing %s in %s\n" "$BINARY_NAME" "$INSTALL_DIR"
fi

mv "${TMPDIR_PATH}/${BINARY_NAME}" "${INSTALL_DIR}/${BINARY_NAME}"
chmod +x "${INSTALL_DIR}/${BINARY_NAME}"

printf "Installed %s to %s\n" "$BINARY_NAME" "${INSTALL_DIR}/${BINARY_NAME}"

# Whether python3 is available *and* its json module can be imported.
# python3-minimal ships without the json module (see issue #41).
has_python_json() {
    command -v python3 >/dev/null 2>&1 && python3 -c "import json" >/dev/null 2>&1
}

merge_with_jq() {
    MERGE_TMP="$(mktemp)"
    jq --arg cmd "${INSTALL_DIR}/${BINARY_NAME}" \
        '.statusLine = {"type": "command", "command": $cmd}' \
        "$SETTINGS_FILE" > "$MERGE_TMP" \
        && mv "$MERGE_TMP" "$SETTINGS_FILE"
}

merge_with_python() {
    python3 -c "
import json, sys
path = sys.argv[1]
with open(path) as f:
    data = json.load(f)
data['statusLine'] = {'type': 'command', 'command': sys.argv[2]}
with open(path, 'w') as f:
    json.dump(data, f, indent=2)
    f.write('\n')
" "$SETTINGS_FILE" "${INSTALL_DIR}/${BINARY_NAME}"
}

write_new_settings() {
    printf '{\n  "statusLine": {\n    "type": "command",\n    "command": "%s"\n  }\n}\n' \
        "${INSTALL_DIR}/${BINARY_NAME}" > "$SETTINGS_FILE"
}

configure_settings() {
    SETTINGS_DIR="$(dirname "$SETTINGS_FILE")"
    mkdir -p "$SETTINGS_DIR"

    if [ ! -f "$SETTINGS_FILE" ]; then
        # No existing settings: write a fresh file, no JSON tooling needed.
        write_new_settings
    elif command -v jq >/dev/null 2>&1; then
        merge_with_jq
    elif has_python_json; then
        merge_with_python
    else
        printf "\nWarning: jq or python3 with the json module is required to update an existing %s.\n" "$SETTINGS_FILE" >&2
        printf "Binary installed but settings not configured. Add this to %s manually:\n" "$SETTINGS_FILE" >&2
        printf '  "statusLine": { "type": "command", "command": "%s" }\n' "${INSTALL_DIR}/${BINARY_NAME}" >&2
        return 0
    fi

    printf "Configured statusLine in %s\n" "$SETTINGS_FILE"
}

configure_settings
