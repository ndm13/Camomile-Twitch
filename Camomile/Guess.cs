using System;
using System.Collections.Generic;
using System.IO;

namespace Camomile
{
    namespace Guess
    {
        static class GameDir
        {
            public static string FromIconPath(string iconPath, string fuelID)
            {
                if (iconPath == null || !iconPath.Contains(fuelID))
                {
                    return null;
                }
                return iconPath.Substring(0, iconPath.IndexOf(fuelID)) + fuelID + '\\';
            }

            public static HashSet<string> FromTwitchHabits(string fuelID)
            {
                var guesses = new HashSet<string>();
                var dirs = new HashSet<string>
                            {
                                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + '\\',
                                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + '\\',
                                "C:\\Program Files (x86)\\"
                            };
                dirs.UnionWith(Environment.GetLogicalDrives());
                foreach (var dir in dirs)
                {
                    var full = dir + "Twitch\\Games Library\\" + fuelID + '\\';
                    if (Directory.Exists(full))
                    {
                        guesses.Add(full);
                    }
                }
                return guesses;
            }
        }



    }
}
