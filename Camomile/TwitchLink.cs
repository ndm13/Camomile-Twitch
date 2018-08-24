using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Camomile
{
    struct TwitchLink
    {

        public TwitchLink(string ini) : this(new Ini(ini), 
            ini.Substring(ini.LastIndexOf('\\') + 1, ini.LastIndexOf('.') - ini.LastIndexOf('\\') - 1)) { }

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
