#!/bin/bash

set -eux

cd "$(dirname "$0")/.."

TEST_RESULTS_DIR="src/StatusLine.Tests/TestResults"
rm -rf "$TEST_RESULTS_DIR"

dotnet test --collect:"XPlat Code Coverage" -p:DebugType=portable

dotnet reportgenerator -reports:"$TEST_RESULTS_DIR/**/coverage.cobertura.xml" -targetdir:coverage-report -reporttypes:Html

set +x

echo -e "\n✨ Report generated: coverage-report/index.html"
