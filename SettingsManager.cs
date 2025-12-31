using System;
using System.IO;

namespace APK_editor_Sigma
{
    internal class SettingsManager
    {
        static string appBaseDir = AppDomain.CurrentDomain.BaseDirectory;
        string settingstxt = Path.Combine(appBaseDir, "Settings.txt");

        public static string TXTpathfile;
        public static bool Anime;

        public void Instantiate()
        {
            if (!File.Exists(settingstxt))
                return;

            string[] lines = File.ReadAllLines(settingstxt);

            foreach (string line in lines)
            {
                if (line.StartsWith("TXT_PATH:"))
                {
                    TXTpathfile = line.Split(':')[1].Trim();
                }
                else if (line.StartsWith("ANIME:"))
                {
                    string value = line.Split(':')[1].Trim();
                    Anime = value.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        public void ChangeAnime(bool status)
        {
            Anime = status;
            UpdateSetting("ANIME", status.ToString().ToLower());
        }

        public void ChangeTXTPath(string path)
        {
            TXTpathfile = path;
            UpdateSetting("TXT_PATH", path);
        }

        private void UpdateSetting(string key, string newValue)
        {
            string[] lines = File.Exists(settingstxt)
                ? File.ReadAllLines(settingstxt)
                : new string[0];

            bool found = false;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith(key + ":"))
                {
                    lines[i] = $"{key}: {newValue}";
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Array.Resize(ref lines, lines.Length + 1);
                lines[1] = $"{key}: {newValue}";
            }

            File.WriteAllLines(settingstxt, lines);
        }
    }
}
