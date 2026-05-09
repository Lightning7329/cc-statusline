#!/bin/sh
set -eu

REPO="OWNER/REPO"
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

if [ -f "${INSTALL_DIR}/${BINARY_NAME}" ]; then
    printf "Replacing existing %s in %s\n" "$BINARY_NAME" "$INSTALL_DIR"
fi

mv "${TMPDIR_PATH}/${BINARY_NAME}" "${INSTALL_DIR}/${BINARY_NAME}"
chmod +x "${INSTALL_DIR}/${BINARY_NAME}"

printf "Installed %s to %s\n" "$BINARY_NAME" "${INSTALL_DIR}/${BINARY_NAME}"

configure_settings() {
    SETTINGS_DIR="$(dirname "$SETTINGS_FILE")"
    mkdir -p "$SETTINGS_DIR"

    if [ -f "$SETTINGS_FILE" ]; then
        python3 -c "
import json, sys
path = sys.argv[1]
with open(path) as f:
    data = json.load(f)
data['statusLine'] = {'type': 'command', 'command': '~/.claude/bin/statusline'}
with open(path, 'w') as f:
    json.dump(data, f, indent=2)
    f.write('\n')
" "$SETTINGS_FILE"
    else
        printf '{\n  "statusLine": {\n    "type": "command",\n    "command": "~/.claude/bin/statusline"\n  }\n}\n' > "$SETTINGS_FILE"
    fi

    printf "Configured statusLine in %s\n" "$SETTINGS_FILE"
}

if command -v python3 >/dev/null 2>&1; then
    configure_settings
else
    printf "\nNote: python3 not found. Add this to %s manually:\n" "$SETTINGS_FILE"
    printf '  "statusLine": { "type": "command", "command": "~/.claude/bin/statusline" }\n'
fi

case ":${PATH}:" in
    *":${INSTALL_DIR}:"*) ;;
    *)
        printf "\nNote: %s is not in your PATH.\n" "$INSTALL_DIR"
        printf "Add it by appending this to your shell profile:\n"
        printf "  export PATH=\"%s:\$PATH\"\n" "$INSTALL_DIR"
        ;;
esac
