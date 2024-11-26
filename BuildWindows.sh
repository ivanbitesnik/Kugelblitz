#!/usr/bin/bash

echo "Enter user password: "
stty -echo
read userpassword

echo $userpassword | sudo -S -k mkdir -m 777 Builds Builds/Windows64
godot --path "Project" --export-debug "Windows desktop" "../Builds/Windows64/Kugelblitz.exe"

stty echo
echo "All done!"
