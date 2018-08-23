using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Camomile
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Camomile - Taking the Edge Off of Twitch");
            Console.WriteLine("========================================");
            Console.WriteLine("Ver.0.5");
            Console.WriteLine();

            Console.WriteLine("Checking for Twitch links...");
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            foreach(var file in Directory.GetFiles(path, "*.url"))
            {
                var data = new Ini(file);
                // Sanity check - should always be true
                if (data.GetSections().Contains("InternetShortcut"))
                {
                    var twitchLink = data.GetValue("URL", "InternetShortcut", null);
                    // Confirm it's actually a Twitch link
                    if(twitchLink != null && new Uri(twitchLink).Scheme == "twitch")
                    {
                        // Get Twitch fuel ID
                        var fuelID = twitchLink.Substring(twitchLink.LastIndexOf('/') + 1);
                        var gameName = file.Substring(file.LastIndexOf('\\') + 1).TrimEnd(".url".ToCharArray());
                        Console.WriteLine("Found Twitch Game: " + gameName);
                        Console.WriteLine("Twitch Fuel ID: " + fuelID);
                        // Check for path
                        string gameDir = null;
                        var iconLink = data.GetValue("IconFile", "InternetShortcut", null);
                        if(iconLink != null && iconLink.Contains(fuelID))
                        {
                            // Get game path from icon path
                            Console.WriteLine("Using icon path to guess game dir...");
                            gameDir = iconLink.Substring(0, iconLink.IndexOf(fuelID) + fuelID.Length + 1);
                            Console.WriteLine("Best guess for game dir: " + gameDir);
                        }
                        else
                        {
                            // Check common game dirs
                            var validDirs = new HashSet<string>();
                            var dirs = new HashSet<string>
                            {
                                // Program Files
                                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + '\\',
                                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + '\\',
                                "C:\\Program Files (x86)\\"
                            };
                            dirs.UnionWith(Environment.GetLogicalDrives());
                            foreach(var dir in dirs)
                            {
                                var full = dir + "Twitch\\Games Library\\" + fuelID + '\\';
                                if (Directory.Exists(full))
                                {
                                    // Found it!  Or, at least an option
                                    validDirs.Add(full);
                                }
                            }
                            var dialog = new FolderBrowserDialog();
                            switch (validDirs.Count)
                            {
                                case 0:
                                    Console.WriteLine("Game dir not found.  Please choose.");
                                    if(dialog.ShowDialog() == DialogResult.OK)
                                    {
                                        gameDir = dialog.SelectedPath;
                                        Console.WriteLine("Manually specified game dir: " + gameDir);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Cancelled path selection, exiting.");
                                        return;
                                    }
                                    break;
                                case 1:
                                    gameDir = validDirs.First();
                                    Console.WriteLine("Best guess for game dir: " + gameDir);
                                    break;
                                default:
                                    Console.WriteLine("Found multiple options.  Please choose the one that looks \"right\".");
                                    foreach(var dir in validDirs)
                                    {
                                        dialog.SelectedPath = dir;
                                        if(dialog.ShowDialog() == DialogResult.OK)
                                        {
                                            gameDir = dialog.SelectedPath;
                                            Console.WriteLine("Chose game dir: " + gameDir);
                                            break;
                                        }
                                    }
                                    if(gameDir == null)
                                    {
                                        Console.WriteLine("No acceptable options?  Choose it yourself, then!");
                                        dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
                                        if(dialog.ShowDialog() == DialogResult.OK)
                                        {
                                            gameDir = dialog.SelectedPath;
                                            Console.WriteLine("Chosen game dir: " + gameDir);
                                        }
                                        else
                                        {
                                            Console.WriteLine("There's no pleasing some people.  C'est la vie...");
                                            return;
                                        }
                                    }
                                    break;
                            }
                        }
                        // We have a game dir at this point; find an exe
                        string exe = null;
                        var exes = Directory.EnumerateFiles(gameDir, "*.exe", SearchOption.AllDirectories).ToArray();
                        switch (exes.Length)
                        {
                            case 0:
                                Console.WriteLine("No exes found!  Sorry, buddy.");
                                return;
                            case 1:
                                exe = exes[0];
                                Console.WriteLine("Found exe: " + exe);
                                break;
                            default:
                                Console.WriteLine("Found some options:");
                                for(var i = 0; i < exes.Length; i++)
                                {
                                    Console.WriteLine(i + ":\t" + exes[i]);
                                }
                                int choice = -1;
                                if (Int32.TryParse(Console.ReadLine(), out choice))
                                {
                                    exe = exes[choice];
                                    Console.WriteLine("Chosen exe: " + exe);
                                }
                                else
                                {
                                    Console.WriteLine("Never mind, then.");
                                    return;
                                }
                                break;
                        }
                        Console.WriteLine("Creating new link...");
                        var shell = new WshShell();
                        IWshShortcut shortcut = shell.CreateShortcut(file.Substring(0, file.LastIndexOf('.')) + " (direct).lnk");
                        shortcut.Description = gameName + " [Twitch]";
                        shortcut.TargetPath = exe;
                        if (iconLink != null)
                        {
                            shortcut.IconLocation = iconLink + ',' + data.GetValue("IconIndex","InternetShortcut","0");
                        }
                        shortcut.Save();
                        Console.WriteLine("Done!");
                    }
                }
            }
            Console.ReadKey();
        }
    }
}
