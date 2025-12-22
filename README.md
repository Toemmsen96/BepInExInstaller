# BepInEx Installer
This program will automatically install BepInEx to Mono-Based games.

## Features
- Install BepInEx into Unity Mono-Games automatically
- Linux: Configure Proton automatically to make BepInEx work (Start the game once first)

## Requirements:
- dotnet runtime 9.0

## Usage
There are multiple ways to use this:
### Using Arguments
To install 
### Placing in in Gamefolder
- place the executable in the Folder, which the game is in.
- Run the executable:

Linux: ```sh ./BepInExInstaller```

Windows: double click executable

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
- Toemmsen96: Linux Version, rewrite, ProtonConfig