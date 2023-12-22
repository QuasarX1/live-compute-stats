#!/bin/bash

script_folder="$(dirname "$(readlink -f "${BASH_SOURCE}")")"

install_location="$1"
if ! [ -z "$install_location" ]; then
    install_location="$install_location"
else
#    install_location="$(readlink -f ./)"
    install_location="./"
fi
install_location_argument="-o \"$install_location\""
#echo "$install_location_argument"
#exit
dotnet publish "$script_folder/src/src.sln" -c Release --no-self-contained $install_location_argument
echo "Installed at \"$(readlink -f "$install_location")\""

cp "$script_folder/TEMPLATE_run.sh" "$install_location/run.sh"
chmod +x "$install_location/run.sh"

if [ $(cat ~/.bashrc | grep "alias server-stats" | wc -l) -eq 0 ]; then
    echo "$(cat ~/.bashrc)

# live-compte-stats server activation alias
alias server-stats=$(readlink -f $install_location)/run.sh" > ~/.bashrc

    echo "Run \"server-stats\" to launch the application. You will need to restart you current terminal session for this to take effect."
fi
