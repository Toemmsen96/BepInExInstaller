# BepInEx Installer
This program will automatically install BepInEx to Mono-Based games.

## Requirements:
- dotnet runtime 9.0

## Usage
- place the executable in the Folder, which the game is in.
- Run the executable:
Linux: ```sh ./BepInExInstaller```
Windows: double click executable
- Follow the instructions in the console

## Build
```sh 
#Linux:
dotnet publish -c Release -r linux-x64
#Windows:
dotnet publish -c Release -r win-x64
```

## Credits
- aedenthorn: Windows Base for this
https://github.com/aedenthorn/BepInExUnityInstaller
- Toemmsen96: Linux Version, rewrite