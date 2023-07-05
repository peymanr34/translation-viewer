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

                string defaultKey = keyPairDefault[0].Trim();
                string defaultValue = keyPairDefault[1].Trim();

                string? keyPairTranslation = linesTranslation
                    .Where(x => x.StartsWith(defaultKey + "="))
                    .FirstOrDefault();

                string? translationValue = keyPairTranslation?.Split('=')[1].Trim();

                var item = new TranslationItem
                {
                    Key = defaultKey,
                    Original = defaultValue,
                    Translation = translationValue,
                };

                if (defaultValue.EndsWith(".") && translationValue?.EndsWith(".") == false)
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

        public static string? GetDefaultTranslationFile()
        {
            var location = GetInstalledLocation();

            if (location is null)
            {
                return null;
            }

            return Path.Join(location, "Default.isl");
        }

        public static string? GetExecutableLocation()
        {
            var location = GetInstalledLocation();

            if (location is null)
            {
                return null;
            }

            return Path.Join(location, "Compil32.exe");
        }

        private static string? GetInstalledLocation()
        {
            // On 64-bit Windows, the registry key for Inno Setup will be accessible under Wow6432Node.
            var keyPath = Environment.Is64BitOperatingSystem
                ? @"SOFTWARE\Wow6432Node\"
                : @"SOFTWARE\";

            var registryKey = Registry.LocalMachine
                .OpenSubKey($@"{keyPath}Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 6_is1");

            if (registryKey is not null)
            {
                return registryKey.GetValue("InstallLocation")?.ToString();
            }

            return null;
        }
    }
}
