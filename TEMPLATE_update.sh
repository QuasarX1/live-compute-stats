#!/bin/bash

cd "$(dirname "$(readlink -f "${BASH_SOURCE}")")"

git clone https://github.com/QuasarX1/live-compute-stats.git /tmp/live-compute-stats
chmod +x /tmp/live-compute-stats/install.sh

/tmp/live-compute-stats/install.sh

echo "
Cleaning up temp files..."
rm /tmp/live-compute-stats -rf
echo "Done."
