#!/bin/bash

cd "$(readlink -f "${BASH_SOURCE}")"

dotnet display-stats.dll
