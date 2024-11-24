#!/usr/bin/bash

echo "Enter user password: "
stty -echo
read userpassword

echo $userpassword | sudo -S -k mkdir -m 777 Builds Builds/Linux64
godot --path "Project" --export-debug "Linux desktop" "../Builds/Linux64/KugelblitzLinux.x86_64"

echo $userpassword | sudo -S -k cp -r "Builds" $(echo $userpassword | sudo -S -k find / -name "steamapps")

stty echo
echo "All done!"
