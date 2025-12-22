using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using BepInExInstaller.ProtonConfig;
using static BepInExInstaller.Installer;

namespace BepInExInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Parse command-line arguments
                string gameName = null;
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-n" && i + 1 < args.Length)
                    {
                        gameName = args[i + 1];
                        break;
                    }
                }

                // If -n flag was provided, find the game directory
                if (!string.IsNullOrEmpty(gameName))
                {
                    Console.WriteLine($"Searching for game: {gameName}");
                    string gameDir = FindGameDirectory(gameName);
                    
                    if (gameDir != null)
                    {
                        Console.WriteLine($"Found game at: {gameDir}");
                        InstallTo(gameDir);
                        Console.WriteLine("\nPress any key to exit...");
                        Console.ReadKey();
                        return;
                    }
                    else
                    {
                        Console.WriteLine($"Could not find game '{gameName}' in Steam libraries.");
                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey();
                        return;
                    }
                }

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

        private static string FindGameDirectory(string gameName)
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string steamPath = Path.Combine(home, ".steam", "steam");

            // First, find the App ID
            int appId = IDFinder.FindGameID(gameName, steamPath);
            if (appId <= 0)
            {
                Console.WriteLine($"Could not find App ID for '{gameName}'");
                return null;
            }

            Console.WriteLine($"Found App ID: {appId}");

            // Get the install directory from appmanifest
            return IDFinder.FindGameInstallDirectory(appId, steamPath);
        }

    }
}
