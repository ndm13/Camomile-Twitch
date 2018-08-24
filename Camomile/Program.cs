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
            var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            foreach(var linkPath in Directory.GetFiles(desktopDir, "*.url"))
            {
                try
                {
                    TwitchLink game = new TwitchLink(linkPath);

                    Console.WriteLine("Guessing based on icon path...");
                    string gameDir = Guess.GameDir.FromIconPath(game.IconPath, game.FuelID);
                    if(gameDir == null){
                        // Check common game dirs
                        var guesses = Guess.GameDir.FromTwitchHabits(game.FuelID);
                        var dialog = new FolderBrowserDialog();
                        switch (guesses.Count)
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
                                gameDir = guesses.First();
                                Console.WriteLine("Best guess for game dir: " + gameDir);
                                break;
                            default:
                                Console.WriteLine("Found multiple options.  Please choose the one that looks \"right\".");
                                foreach(var dir in guesses)
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
                    game.WriteStandardLink(exe, desktopDir + "\\_" + game.Name + ".lnk");
                    Console.WriteLine("Done!");
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine("Skipping link " + linkPath + "\n\t" + e.Message);
                }
            }
            Console.ReadKey();
        }
    }
}
