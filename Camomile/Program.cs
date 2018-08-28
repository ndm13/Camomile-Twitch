using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Camomile
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Camomile - Taking the Edge Off of Twitch");
            Console.WriteLine("========================================");
            Console.WriteLine("Ver.0.8");
            Console.WriteLine();

            Console.WriteLine("Checking for Twitch links...");
            var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            foreach(var linkPath in Directory.GetFiles(desktopDir, "*.url"))
            {
                try
                {
                    var link = new TwitchLink(linkPath);
                    Console.WriteLine("Found new game: " + link.Name);
                    Console.WriteLine("Guessing game dir...");
                    Console.WriteLine("Guessing based on icon path...");
                    string gameDir = Guess.GameDir.FromIconPath(link.IconPath, link.FuelID);
                    if(gameDir == null){
                        Console.WriteLine("Guessing based on Twitch defaults...");
                        var guesses = Guess.GameDir.FromTwitchHabits(link.FuelID).ToArray();
                        if (guesses.Length == 1)
                        {
                            gameDir = guesses[0];
                        }
                        else if(guesses.Length > 1)
                        {
                            Console.WriteLine("Found multiple options.  Please choose the one that looks \"right\".");
                            for (var i = 0; i < guesses.Length; i++)
                                Console.WriteLine(i + ":\t" + guesses[i]);
                            if (int.TryParse(Console.ReadLine(), out int choice))
                            {
                                gameDir = guesses[choice];
                            }
                        }
                    }
                    var folderPicker = new FolderBrowserDialog();
                    if (gameDir == null)
                    {
                        Console.WriteLine("We couldn't guess which folder your game is in.  Find it for us?");
                        folderPicker.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
                        if(folderPicker.ShowDialog() == DialogResult.OK)
                        {
                            gameDir = folderPicker.SelectedPath;
                        }
                        else
                        {
                            Console.WriteLine("Okay, we'll skip this game then.");
                            continue;
                        }
                    }
                    // We have a game dir at this point; find an exe
                    Console.WriteLine("Guessing exe path from fuel.json...");
                    var exe = Guess.ExePath.FromFuelJson(gameDir);
                    if (exe == null)
                    {
                        Console.WriteLine("Guessing exe path from Twitch defaults...");
                        var exes = Guess.ExePath.FromBasicLogic(gameDir).ToArray();
                        switch (exes.Length)
                        {
                            case 0:
                                Console.WriteLine("No exes found!  Sorry, buddy.");
                                return;
                            case 1:
                                exe = exes[0];
                                break;
                            default:
                                Console.WriteLine("Found some options:");
                                for (var i = 0; i < exes.Length; i++)
                                {
                                    Console.WriteLine(i + ":\t" + exes[i]);
                                }

                                var iconGuess = Guess.ExePath.FromIconPath(link.IconPath);
                                if (iconGuess != null)
                                    Console.WriteLine("Note: Icon guess is " + iconGuess);

                                if (int.TryParse(Console.ReadLine(), out int choice))
                                {
                                    exe = exes[choice];
                                }
                                else
                                {
                                    Console.WriteLine("Never mind, then.");
                                    continue;
                                }
                                break;
                        }
                    }
                    Console.WriteLine("Found exe: " + exe);
                    Console.WriteLine("Creating new link...");
                    link.WriteStandardLink(exe, desktopDir + "\\_" + link.Name + ".lnk");
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
