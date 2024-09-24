#!/bin/sh
echo -ne '\033c\033]0;Kugelblitz\a'
base_path="$(dirname "$(realpath "$0")")"
"$base_path/Kugelblitz.x86_64" "$@"
