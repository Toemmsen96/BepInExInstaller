# BepInEx Installer
This program will automatically install BepInEx to Mono and IL2CPP-Based games. 
This is built for Windows games, so it doesn't install to Linux native binaries (yet), but to games running proton.

## Features
- Install BepInEx into Unity-Games automatically
- For Linux: Configure Proton automatically to make BepInEx work (Steam: Start the game once first to create Proton prefix)
- For IL2CPP-Games: Installs the latest BepInEx 6 - Bleeding edge-build

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
Follow along with what the Installer tells you. It will ask you if you want to configure Proton to make BepInEx work.

### Placing in in Gamefolder (Any Games)
- place the executable in the Folder, which the game is in.
- Run the executable:

Linux: 
```sh 
./BepInExInstaller
```

Windows: double click the executable

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
[BepInExUnityInstaller](https://github.com/aedenthorn/BepInExUnityInstaller)
- Toemmsen96: Linux Version, rewrite, ProtonConfig, IL2CPP Support
