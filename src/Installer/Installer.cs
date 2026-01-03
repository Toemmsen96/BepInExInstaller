using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using BepInExInstaller.ProtonConfig;
namespace BepInExInstaller
{
    public static class Installer
    {
        internal static ConsoleKeyInfo key;
        public static void InstallTo(string gamePath)
        {
            ConsoleKeyInfo key;

            Console.WriteLine("Looking for BepInEx archive...");

            string path = AppContext.BaseDirectory;

            bool x64 = true;
            foreach (string file in Directory.GetFiles(path, "*.exe"))
            {
                if (!file.StartsWith("BepInEx") && Directory.Exists(Path.Combine(path, Path.GetFileNameWithoutExtension(file) + "_Data")))
                {
                    Console.WriteLine($"Basing architecture on {file}: {(GetAppCompiledMachineType(file) == MachineType.x86 ? "32-bit" : "64-bit")}");
                    x64 = GetAppCompiledMachineType(file) != MachineType.x86;
                }
            }

            Console.WriteLine($"Game appears to be {(x64 ? "64-bit" : "32-bit")}...");

            if (IsIl2CppGame(gamePath))
            {
                Console.WriteLine("IL2CPP game detected! This installer cannot (yet?) download BepInEx for IL2CPP games.");
                if (x64)
                    Console.WriteLine("Please Download BepInEx-Unity.IL2CPP-win-x64...zip manually from here: https://builds.bepinex.dev/projects/bepinex_be");
                else
                    Console.WriteLine("Please Download BepInEx-Unity.IL2CPP-win-x86...zip manually from here: https://builds.bepinex.dev/projects/bepinex_be");
                Console.WriteLine("You can still Check and Configure Proton for this game. Do you want to continue? Y/n");
                key = Console.ReadKey();
                if (!(key.Key == ConsoleKey.Y || key.Key == ConsoleKey.Enter))
                {
                    return;
                }
                CheckAndConfigureProton(gamePath);
                return;
            }

            string zipPath = null;
            foreach (string file in Directory.GetFiles(path, "*.zip"))
            {
                if ((x64 && Path.GetFileName(file).StartsWith("BepInEx_win_x64")) || (!x64 && Path.GetFileName(file).StartsWith("BepInEx_win_x86")))
                {
                    zipPath = file;
                    break;
                }
            }
            if (zipPath != null)
            {
                Console.WriteLine($"Existing archive found at {zipPath}");
                Console.WriteLine($"Use this archive? Y/n");
                key = Console.ReadKey();
                if (key.Key == ConsoleKey.N)
                {
                    zipPath = null;
                }
            }
            else
            {
                Console.WriteLine("BepInEx zip file not found...");
            }
            if (zipPath == null)
            {
                Console.WriteLine("Downloading BepInEx from GitHub...");
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "request");
                    string source = client.GetStringAsync("https://api.github.com/repos/BepInEx/BepInEx/releases/latest").Result;
                    var match = Regex.Match(source, "(https://github.com/BepInEx/BepInEx/releases/download/v[^/]+/BepInEx_win_[^\"]*" + (x64 ? "x64" : "x86") + "[^\"]+)\"");
                    if (!match.Success)
                    {
                        Console.WriteLine("Couldn't find latest BepInEx file, please visit https://github.com/BepInEx/BepInEx/releases/ to download the latest release.");
                        Console.ReadKey();
                        return;
                    }

                    string latest = match.Groups[1].Value;
                    Console.WriteLine($"Downloading {latest}");
                    string fileName = Path.GetFileName(latest);
                    zipPath = Path.Combine(path, fileName);
                    var data = client.GetByteArrayAsync(latest).Result;
                    File.WriteAllBytes(zipPath, data);
                    Console.WriteLine($"Downloaded {latest}");

                }
            }

            if (!File.Exists(zipPath))
            {
                Console.WriteLine($"Zip file {zipPath} does not exist!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Installing BepInEx...");

            var archive = ZipFile.OpenRead(zipPath);
            foreach (var entry in archive.Entries)
            {
                string f = Path.Combine(gamePath, entry.FullName);
                if (!Directory.Exists(Path.GetDirectoryName(f)))
                    Directory.CreateDirectory(Path.GetDirectoryName(f));
                entry.ExtractToFile(Path.Combine(gamePath, entry.FullName), true);
                Console.WriteLine($"Copying {entry.FullName}");
            }
            archive.Dispose();

            if (!Directory.Exists(Path.Combine(gamePath, "BepInEx", "plugins")))
                Directory.CreateDirectory(Path.Combine(gamePath, "BepInEx", "plugins"));

            Console.WriteLine($"BepInEx installed to {gamePath}!");
            Console.WriteLine("Delete downloaded zip archive? Y/n");
            key = Console.ReadKey();
            if (key.Key != ConsoleKey.N)
            {
                File.Delete(zipPath);
            }
            Console.WriteLine($"");
            Console.WriteLine($"Installation Complete!");
            
            CheckAndConfigureProton(gamePath);
            }

        private static void CheckAndConfigureProton(string gamePath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("");
                Console.WriteLine("Linux detected! Would you like to configure Proton for this game? Y/n");
                Console.WriteLine("This will set the winhttp override required for BepInEx to work.");
                key = Console.ReadKey();
                Console.WriteLine("");
                
                if (key.Key == ConsoleKey.Y || key.Key == ConsoleKey.Enter)
                {
                    // Use directory name (from Steam install dir) as it matches the appmanifest game name
                    string gameName = Path.GetFileName(gamePath);
                    
                    Console.WriteLine($"Attempting to find Steam App ID for '{gameName}'...");
                    
                    string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    string steamPath = Path.Combine(home, ".steam", "steam");
                    
                    int appId = IDFinder.FindGameID(gameName, steamPath);
                    
                    if (appId > 0)
                    {
                        Console.WriteLine($"Found Steam App ID: {appId}");
                        Console.WriteLine($"Use this App ID? Y to confirm, N to enter manually:");
                        key = Console.ReadKey();
                        Console.WriteLine("");
                        
                        if (key.Key == ConsoleKey.N)
                        {
                            Console.WriteLine("Please enter the Steam App ID for this game:");
                            string manualAppId = Console.ReadLine();
                            if (!string.IsNullOrWhiteSpace(manualAppId) && int.TryParse(manualAppId, out int parsedId))
                            {
                                appId = parsedId;
                            }
                            else
                            {
                                Console.WriteLine("Invalid App ID. Skipping Proton configuration.");
                                appId = -1;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Could not automatically find App ID for '{gameName}'.");
                        Console.WriteLine("Please enter the Steam App ID manually (or press Enter to skip):");
                        string manualAppId = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(manualAppId) && int.TryParse(manualAppId, out int parsedId))
                        {
                            appId = parsedId;
                        }
                    }
                    
                    if (appId > 0)
                    {
                        Console.WriteLine($"Configuring Proton for Steam App ID {appId}...");
                        int result = ProtonConfig.ProtonConfig.Execute(appId.ToString(), "winhttp");
                        
                        if (result == 0)
                        {
                            Console.WriteLine("Proton configuration completed successfully!");
                        }
                        else
                        {
                            Console.WriteLine("Proton configuration failed. You may need to configure it manually.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid App ID. Skipping Proton configuration.");
                    }
                }
        }
        }

        public static bool IsIl2CppGame(string gamePath)
        {
            if (Directory.Exists(Path.Combine(gamePath, "il2cpp_data")))
                return true;
            
            // Search in subfolders
            try
            {
                string[] subdirs = Directory.GetDirectories(gamePath, "il2cpp_data", SearchOption.AllDirectories);
                if (subdirs.Length > 0)
                    return true;
            }
            catch
            {
                // Ignore exceptions (e.g., permission issues)
            }
            
            return false;
        }

            public static MachineType GetAppCompiledMachineType(string fileName)
        {
            const int PE_POINTER_OFFSET = 60;
            const int MACHINE_OFFSET = 4;
            byte[] data = new byte[4096];
            using (Stream s = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                s.Read(data, 0, 4096);
            }
            // dos header is 64 bytes, last element, long (4 bytes) is the address of the PE header
            int PE_HEADER_ADDR = BitConverter.ToInt32(data, PE_POINTER_OFFSET);
            int machineUint = BitConverter.ToUInt16(data, PE_HEADER_ADDR + MACHINE_OFFSET);
            return (MachineType)machineUint;
        }

    }
}
