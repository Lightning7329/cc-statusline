#!/bin/bash

FILE=$(jq -r '.tool_input.file_path')

case "$FILE" in
  *.fs) ;;
  *) exit 0 ;;
esac

dotnet fantomas "$FILE"
