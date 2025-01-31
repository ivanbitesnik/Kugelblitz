linux:
	echo "Starting build for Linux"
	mkdir builds/linux
	godot --headless --path "Project" --export-release "Linux desktop" "../builds/linux/kugel.x86_64"
	echo "All Done!"

win: 
	echo "Starting build for Windows"
	mkdir builds/windows
	godot --headless --path "Project" --export-release "Windows desktop" "../builds/windows/kugel.exe"
	echo "All Done!"

all: linux win

allclean: clean linux win

clean:
	rm -r "./builds/linux"
	rm -r "./builds/windows"

