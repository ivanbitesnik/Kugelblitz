#!/usr/bin/bash

echo "Enter user password: "
stty -echo
read userpassword
stty echo

echo $userpassword | sudo -S -k mkdir -m 777 Builds Builds/Linux64
godot --path "Project" --export-debug "Linux desktop" "../Builds/Linux64/KugelblitzLinux.x86_64"

sudo cp -r "Builds" $(sudo find / -name "steamapps")
