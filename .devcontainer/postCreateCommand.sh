#!/bin/bash

set -eux

dotnet tool restore

dotnet restore

# ~/.claude.json の実体を ~/.claude/ 配下に置き、シンボリックリンクで参照させる
mkdir -p ~/.claude
if [[ -f ~/.claude.json && ! -L ~/.claude.json ]]; then
    mv ~/.claude.json ~/.claude/config.json
elif [[ ! -f ~/.claude.json ]]; then
    touch ~/.claude/config.json
fi
if [[ ! -L ~/.claude.json ]]; then
    ln -s ~/.claude/config.json ~/.claude.json
fi
