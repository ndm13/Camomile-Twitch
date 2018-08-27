using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Camomile
{
    /// <summary>
    /// Represents a Twitch fuel link: e.g. one of the .url files on the
    /// desktop that we're trying to replace.  Contains a lot of useful
    /// constructors.
    /// </summary>
    struct TwitchLink
    {
        /// <summary>
        /// Builds a Twitch link from a .url file path.
        /// 
        /// Throws ArgumentException on invalid data.
        /// </summary>
        /// <param name="ini">The path of the .url file.</param>
        public TwitchLink(string ini) : this(new Ini(ini), 
            ini.Substring(ini.LastIndexOf('\\') + 1, ini.LastIndexOf('.') - ini.LastIndexOf('\\') - 1)) { }

        /// <summary>
        /// Builds a Twitch link from ini data from a .url, with an optional
        /// file name.
        /// 
        /// Throws ArgumentException on invalid data.
        /// </summary>
        /// <param name="ini">The data from the .url file, in ini format.</param>
        /// <param name="name">The name of the game (defaults to null).</param>
        public TwitchLink(Ini ini, string name = null)
        {
            if (!ini.GetSections().Contains("InternetShortcut"))
            {
                throw new ArgumentException("Not a valid shortcut");
            }
            Name = name;
            try
            {
                FuelID = GetFuelIDFromUri(ini.GetValue("URL", "InternetShortcut", null));
                IconPath = ini.GetValue("IconFile", "InternetShortcut", null);
                IconID = int.Parse(ini.GetValue("IconIndex", "InternetShortcut", "0"));
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Illegal values in shortcut; are you sure it's Twitch?", e);
            }
        }

        /// <summary>
        /// Directly creates a Twitch link without messing around with parsing.
        /// </summary>
        /// <param name="name">Name of the game.</param>
        /// <param name="fuelID">Twitch fuel ID.</param>
        /// <param name="iconPath">Icon path for the game.</param>
        /// <param name="iconID">Index of the icon for the game.</param>
        public TwitchLink(string name, string fuelID, string iconPath, int iconID)
        {
            Name = name;
            FuelID = fuelID;
            IconPath = iconPath;
            IconID = iconID;
        }

        public string Name { get; set; }
        public string FuelID { get; set; }
        public Uri FuelURL
        {
            get => FuelID == null ? null : new Uri("twitch://fuel-launch/" + FuelID + '/');
            set => FuelID = GetFuelIDFromUri(value);
        }
        public string IconPath { get; private set; }
        public int IconID { get; private set; }

        /// <summary>
        /// Creates a .lnk at the specified location, using the existing
        /// metadata, to the specified exe.
        /// </summary>
        /// <param name="exePath">Where the link should point to.</param>
        /// <param name="linkPath">Where the link should exist.</param>
        public void WriteStandardLink(string exePath, string linkPath)
        {
            var shell = new WshShell();
            var shortcut = shell.CreateShortcut(linkPath);
            shortcut.Description = Name;
            shortcut.TargetPath = exePath;
            if (IconPath != null)
            {
                shortcut.IconLocation = IconPath + ',' + IconID;
            }
            shortcut.Save();
        }

        public static string GetFuelIDFromUri(string uri) => GetFuelIDFromUri(new Uri(uri));

        public static string GetFuelIDFromUri(Uri uri)
        {
            if(uri == null)
            {
                throw new ArgumentNullException("URI must not be null");
            }
            if(uri.Scheme != "twitch" || uri.Host != "fuel-launch")
            {
                throw new ArgumentException("Must be a Twitch Fuel URI: " + uri.ToString());
            }
            return uri.AbsolutePath.Trim('/');
        }
    }
}
