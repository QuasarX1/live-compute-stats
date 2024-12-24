param([String]$install_location=".\")

$install_location = Resolve-Path -Path "$install_location"
$script_folder = Resolve-Path -Path "$PSScriptRoot"

dotnet publish "$script_folder/src/src.sln" -c Release --no-self-contained -o $install_location
Write-Output "Installed at ""$install_location"""

Copy-Item "$script_folder/TEMPLATE_run.sh" "$install_location/run.sh"

Copy-Item "$script_folder/TEMPLATE_update.sh" "$install_location/update.sh"
