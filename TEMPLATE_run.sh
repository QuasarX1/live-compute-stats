#!/bin/bash

cd "$(dirname "$(readlink -f "${BASH_SOURCE}")")"

dotnet display-stats.dll
