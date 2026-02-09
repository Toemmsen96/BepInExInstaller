# BepInEx Installer
This program will automatically install BepInEx to Mono and IL2CPP-Based games. 
This is built for Windows games, so it doesn't install to Linux native binaries (yet), but to games running proton.

**GUI Edition:** https://github.com/Toemmsen96/BepInExInstallerGUI

## Features
- Install BepInEx into Unity-Games automatically
- Validates game directory structure (requires GameName.exe and GameName_Data folder)
- Automatically detects game architecture (32-bit or 64-bit)
- For Linux: Configure Proton automatically to make BepInEx work (Steam: Start the game once first to create Proton prefix)
- For IL2CPP-Games: Installs the latest BepInEx 6 - Bleeding edge-build
- Optional: Enable BepInEx console logging automatically with `-c` flag
- Verbose output mode for detailed installation information with `-v` flag
- Install BepInEx plugins using `-i <archive.zip>` flag

## Requirements:
- dotnet runtime 9.0
- Steam (for Installation using -n on Linux and Proton config)

## Usage
There are multiple ways to use this:

### Using Arguments (Steam games)
First Start the Game once, to generate the Proton Prefix.

After that, to install BepInEx into Steam games from anywhere:
```sh
./BepInExInstaller -n "Game Name"
```

**Available Options:**
- `-n <game_name>` - Specify the name of the game to install BepInEx for (installer will locate the game in Steam libraries)
- `-i <archive.zip>` - Specify a zip archive containing plugins/mods to install into BepInEx/plugins folder
- `-c` or `--console` - Enable BepInEx console logging by setting Enabled=true in BepInEx.cfg
- `-v` or `--verbose` - Enable verbose output during installation for detailed information
- `-h` or `--help` - Display help message

**Example Commands:**
```sh
# Install BepInEx with console enabled and verbose output
./BepInExInstaller -n "Game Name" -c -v

# Install BepInEx and plugins from archive
./BepInExInstaller -n "Game Name" -i plugins.zip

# Install with plugins, console enabled, and verbose output
./BepInExInstaller -n "Game Name" -i mods.zip -c -v

# Install with just console enabled
./BepInExInstaller -n "Game Name" --console

# Install with verbose output only
./BepInExInstaller -v -n "Game Name"
```

Follow along with what the Installer tells you. It will ask you if you want to configure Proton to make BepInEx work.

### Placing in in Gamefolder (Any Games)
- place the executable in the Folder, which the game is in.
- Run the executable:

Linux: 
```sh 
# Basic installation
./BepInExInstaller

# With console and verbose output
./BepInExInstaller -c -v

# Install with plugins
./BepInExInstaller -i plugins.zip -c
```

Windows: double click the executable, or use command line with options:
```cmd
BepInExInstaller.exe -c -v

# With plugins
BepInExInstaller.exe -i mods.zip -c -v
```

- Follow the instructions in the console

## Build

**Requirements:**
- .NET SDK 9.0

**Build:**

```sh 
#Linux:
dotnet publish -c Release -r linux-x64
#Windows:
dotnet publish -c Release -r win-x64
```

## Credits
- aedenthorn: Windows Base for this:
- [BepInExUnityInstaller(GitHub)](https://github.com/aedenthorn/BepInExUnityInstaller)
- [BepInExUnityInstaller(NexusMods)](https://www.nexusmods.com/site/mods/287)
- [Toemmsen96](https://github.com/Toemmsen96): Linux Version, rewrite, ProtonConfig, IL2CPP Support, Commandline Arguments / extra functions
