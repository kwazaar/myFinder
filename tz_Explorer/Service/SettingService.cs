using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tz_Explorer.Service
{
    public static class SettingsService
    {
        private static Properties.Settings Settings
        {
            get { return Properties.Settings.Default; }
        }
        public static void SaveDefaultDirectoryPath(string path)
        {
            Settings.DefaultDirectoryPath = path;
            Settings.Save();
        }
        public static string GetDefaultDirectoryPath()
        {
            if (string.IsNullOrEmpty(Settings.DefaultDirectoryPath))
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            return Settings.DefaultDirectoryPath;
        }
    }
}
