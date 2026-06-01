using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using TranslationViewer.Models;

namespace TranslationViewer.Utilities
{
    public static class InnoSetupProvider
    {
        public static IEnumerable<TranslationItem> GetTranslationItems(string[] linesDefault, string[] linesTranslation)
        {
            for (int i = 0; i < linesDefault.Length; i++)
            {
                string line = linesDefault[i];

                // Skip on comments.
                if (string.IsNullOrWhiteSpace(line) || line.Contains('=') == false || line.StartsWith(';'))
                {
                    continue;
                }

                var keyPairDefault = line.Split('=');

                var item = new TranslationItem
                {
                    Key = keyPairDefault[0].Trim(),
                    Original = keyPairDefault[1].Trim(),
                };

                string? keyPairTranslation = linesTranslation
                    .Where(x => x.StartsWith(item.Key + "="))
                    .FirstOrDefault();

                item.Translation = keyPairTranslation?.Split('=')[1].Trim();

                if (!string.IsNullOrEmpty(item.Original) &&
                    string.IsNullOrEmpty(item.Translation))
                {
                    item.HasErrors = true;
                }

                if (item.Original.EndsWith(".") &&
                    item.Translation?.EndsWith(".") == false)
                {
                    item.HasErrors = true;
                }

                yield return item;
            }
        }

        public static string? BrowseForFileOrDefault()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Inno Setup Language File (*.isl)|*.isl",
            };

            var location = GetInstalledLocation();

            if (Directory.Exists(location))
            {
                dialog.CustomPlaces.Add(new FileDialogCustomPlace(location));
            }

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }

            return null;
        }

        public static string? GetInstalledLocation()
        {
            foreach (var keyPath in GetRegistryKeyPaths())
            {
                using var key = Registry.LocalMachine.OpenSubKey(keyPath)
                    ?? Registry.CurrentUser.OpenSubKey(keyPath);

                if (key?.GetValue("InstallLocation") is string path)
                {
                    return path;
                }
            }

            return null;
        }

        private static IEnumerable<string> GetRegistryKeyPaths()
        {
            // Inno Setup 7 (64-bit) on 64-bit Windows or Inno Setup 7 (32-bit) on 32-bit Windows.
            yield return GetRegistryKeyPath("Inno Setup 7_is1");
            // Inno Setup 7 (32-bit) on 64-bit Windows.
            yield return GetRegistryKeyPath("Inno Setup 7_is1", true);
            // Inno Setup 6 is 32-bit only.
            yield return GetRegistryKeyPath("Inno Setup 6_is1", Environment.Is64BitOperatingSystem);
        }

        private static string GetRegistryKeyPath(string relativePath, bool useWow6432 = false)
        {
            var baseKeyPath = "SOFTWARE\\";

            if (useWow6432)
            {
                baseKeyPath += "Wow6432Node\\";
            }

            return $"{baseKeyPath}Microsoft\\Windows\\CurrentVersion\\Uninstall\\{relativePath}";
        }
    }
}
