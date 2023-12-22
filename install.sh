#!/bin/bash

script_folder="$(readlink -f "${BASH_SOURCE}")"
install_location="$1"
if ! [ -z "install_location" ]; then
	install_location="$(readlink -f "$install_location")"
else
	install_location="$script_folder"
fi
install_location_argument="-o \"$install_location\""

dotnet publish "$script_folder/src/src.sln" -c Release --no-self-contained $install_location
cp "$script_folder/TEMPLATE_run.sh" "$install_location/run.sh"
chmod +x "$install_location/run.sh"

echo "$(cat ~/.bashrc)

alias =$install_location/run.sh" > ~/.bashrc
