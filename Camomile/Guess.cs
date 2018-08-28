using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Camomile
{
    namespace Guess
    {
        static class GameDir
        {
            /// <summary>
            /// Tries to get the game folder from the icon path, based on the
            /// assumption that the icon for the game is stored in the game
            /// folder and that the folder is named after the Twitch fuel ID.
            /// This is Twitch's convention, so it's pretty safe.
            /// 
            /// Note: This path ends with a backslash.
            /// </summary>
            /// <param name="iconPath">The path to the icon for the game.</param>
            /// <param name="fuelID">The fuel ID for the game.</param>
            /// <returns></returns>
            public static string FromIconPath(string iconPath, string fuelID)
            {
                if (iconPath == null || !iconPath.Contains(fuelID))
                {
                    return null;
                }
                return iconPath.Substring(0, iconPath.IndexOf(fuelID)) + fuelID + '\\';
            }

            /// <summary>
            /// Searches the common folders that Twitch tries to put your games
            /// into.  This list includes, by default:
            /// - [Program Files X86]\Twitch\Games Library
            /// - [Program Files]\Twitch\Games Library
            /// - C:\Program Files (x86)\Twitch\Games Library (literal path)
            /// - [every root drive on the system]\Twitch\Games Library
            /// </summary>
            /// <param name="fuelID">The fuel ID of the game.</param>
            /// <returns>All valid (read: existing) guesses for the game dir.</returns>
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

        static class ExePath
        {
            public static string FromFuelJson(string gameDir)
            {
                if(!File.Exists(gameDir + "fuel.json"))
                {
                    return null;
                }
                var json = JObject.Parse(File.ReadAllText(gameDir + "fuel.json"));
                try
                {
                    if (json.Value<string>("SchemaVersion") == "2")
                    {
                        if (json.TryGetValue("Main", out JToken main) && (main as JObject).ContainsKey("Command"))
                        {
                            return gameDir + main.Value<string>("Command");
                        }
                    }
                }
                catch { }
                return null;
            }

            /// <summary>
            /// Yeah, it's kinda dumb, but most Twitch games use the icon bound
            /// into the exe itself, so we actually have a pretty good chance
            /// of getting an accurate path.
            /// </summary>
            /// <param name="iconPath">The path of the icon for the game.</param>
            /// <returns>The same path if it's a valid exe, or null otherwise.</returns>
            public static string FromIconPath(string iconPath)
            {
                if (iconPath.EndsWith(".exe"))
                {
                    return iconPath;
                }
                return null;
            }

            /// <summary>
            /// Looks for exes in the game directory while trying to rule out
            /// common bad choices.  Currently this list is:
            /// - *dxsetup.exe - DirectX setup program (support exe)
            /// - *vcredist* - Visual C redist package
            /// - *setup* - typically a setup program
            /// - *unins* - typically an uninstaller
            /// </summary>
            /// <param name="gameDir">The game files directory.</param>
            /// <returns>A HashSet of potential options.</returns>
            public static HashSet<string> FromBasicLogic(string gameDir)
            {
                var all = Directory.EnumerateFiles(gameDir, "*.exe", SearchOption.AllDirectories);
                var filtered = new HashSet<string>();
                foreach(var exe in all)
                {
                    var sptl = exe.Substring(gameDir.Length).ToLower();
                    // Setup Support Files
                    if (sptl.EndsWith("dxsetup.exe") ||
                        sptl.Contains("vcredist") ||
                        sptl.Contains("setup"))
                        continue;
                    // Uninstaller - usually unins000 or uninstall
                    if (sptl.Contains("unins"))
                        continue;
                    filtered.Add(exe);
                }
                return filtered;
            }
        }

    }
}
