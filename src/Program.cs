using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BepInExInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (File.Exists(Path.Combine(AppContext.BaseDirectory, "UnityPlayer.dll")))
                {
                    Console.WriteLine("Installer is in game folder.");
                    if (Directory.Exists(Path.Combine(AppContext.BaseDirectory, "BepInEx")))
                    {
                        Console.WriteLine("BepInEx folder already exists!");
                        Console.WriteLine("Press U to uninstall or Y to install anyway:");
                        ConsoleKeyInfo key = Console.ReadKey();
                        if (key.Key == ConsoleKey.U)
                        {
                            Console.WriteLine("This will remove all existing BepInEx data and any plugins already installed! Press Y if you're sure:");
                            key = Console.ReadKey();
                            if (key.Key == ConsoleKey.Y)
                            {
                                Console.WriteLine("Deleting BepInEx folder");
                                Directory.Delete(Path.Combine(AppContext.BaseDirectory, "BepInEx"), true);
                                Console.WriteLine("Deleting winhttp.dll");
                                File.Delete(Path.Combine(AppContext.BaseDirectory, "winhttp.dll"));
                                Console.WriteLine("Deleting doorstop_config.ini");
                                File.Delete(Path.Combine(AppContext.BaseDirectory, "doorstop_config.ini"));
                                Console.WriteLine("Deleting changelog.txt");
                                File.Delete(Path.Combine(AppContext.BaseDirectory, "changelog.txt"));
                                Console.WriteLine("\nBepInEx uninstalled! Press any key to exit...");
                            }
                            else
                            {
                                Console.WriteLine("Uninstall aborted! Press any key to exit...");
                            }
                        }
                        else if (key.Key == ConsoleKey.Y)
                        {
                            InstallTo(AppContext.BaseDirectory);
                            Console.WriteLine("\nPress any key to exit...");
                        }
                        Console.ReadKey();
                        return;
                    }

                    InstallTo(AppContext.BaseDirectory);
                    Console.WriteLine("\nPress any key to exit...");
                    Console.ReadKey();
                    return;
                }
                Console.WriteLine("Game folder not found! Install here anyway? (Y to confirm)");
                var keyinfo = Console.ReadKey();
                if (keyinfo.Key == ConsoleKey.Y)
                {
                    InstallTo(AppContext.BaseDirectory);
                    Console.WriteLine("\nPress any key to exit...");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("\n\nPress any key to exit...");
                Console.ReadKey();
            }
        }

        private static void InstallTo(string gamePath)
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
                    var data = client.GetByteArrayAsync(latest).Result;
                    File.WriteAllBytes(fileName, data);
                    zipPath = Path.Combine(path, fileName);
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
        }

        public enum MachineType { Native = 0, x86 = 0x014c, Itanium = 0x0200, x64 = 0x8664 }

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
