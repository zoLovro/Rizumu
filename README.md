# Rizumu

A free-to-win, osu! inspired rhythm game.

# Status
This project is under developement, but a version 1 has been released.



# Download
Head to the [Latest release](https://github.com/zoLovro/Rizumu/releases/tag/PreRelease) and download the file for your operating system.

## Additional instructions for Linux users
If you are using a Linux machine, you will have to download ffmpeg into your root folder (preferably using a package manager).

### Arch Linux / Manjaro

```bash
sudo pacman -S ffmpeg
```

### Fedora

```bash
sudo dnf install https://download1.rpmfusion.org/free/fedora/rpmfusion-free-release-$(rpm -E %fedora).noarch.rpm
sudo dnf install ffmpeg
```

### Ubuntu

```bash
sudo apt update
sudo apt install ffmpeg
```

### Mint

```bash
sudo apt update
sudo apt install ffmpeg
```
If your distro is not on the list, use the appropriate package manager.


# Map importing
1. Launch the application and press the Play button (this creates the Assets directory)
2. Head to the [osu! beatmaps website](https://osu.ppy.sh/beatmapsets) and download any **mania** map
3. Move the .osz file into **Assets/Songs** and extract it 
4. Remove the .osz file
5. Head back into the game and enjoy!

A more modern way is still under developement.

## Licence
The code and framework for Rizumu are lincenced under the MIT licence.