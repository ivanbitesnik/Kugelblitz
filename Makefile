linux:
	echo "Starting build for Linux"
	mkdir -p builds/linux
	godot --headless --path "Project" --export-release "Linux" "../builds/linux/kugel.x86_64"
	echo "All Done!"

win: 
	echo "Starting build for Windows"
	mkdir -p builds/windows
	godot --headless --path "Project" --export-release "Windows" "../builds/windows/kugel.exe"
	echo "All Done!"

all: linux win

allclean: clean linux win

clean:
	rm -r "./builds/linux"
	rm -r "./builds/windows"

