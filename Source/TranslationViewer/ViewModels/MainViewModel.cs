using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using MvvmGen;
using TranslationViewer.Models;
using TranslationViewer.Utilities;

namespace TranslationViewer.ViewModels
{
    [ViewModel]
    public partial class MainViewModel
    {
        private string? _compilerLocation;

        [Property]
        private string? _defaultPath;

        [Property]
        private string? _translationPath;

        [Property]
        private bool _isTranslationRightToLeft;

        [Property]
        private ObservableCollection<TranslationItem> _items = [];

        public int TotalCount
            => _items?.Count ?? 0;

        public int TotalErrors
            => _items?.Count(x => x.HasErrors) ?? 0;

        public DateTime? TranslationLastWriteTime { get; private set; }

        public void Initialize()
        {
            _compilerLocation = InnoSetupProvider.GetInstalledLocation();

            if (Path.Exists(_compilerLocation))
            {
                DefaultPath ??= Path.Join(_compilerLocation, "Default.isl");
            }
        }

        [Command]
        private void BrowseDefault()
        {
            var selectedFile = InnoSetupProvider.BrowseForFileOrDefault();

            if (selectedFile is null)
            {
                return;
            }

            DefaultPath = selectedFile;
            Load();
        }

        [Command]
        private void BrowseTranslation()
        {
            var selectedFile = InnoSetupProvider.BrowseForFileOrDefault();

            if (selectedFile is null)
            {
                return;
            }

            TranslationPath = selectedFile;
            Load();
        }

        [Command(CanExecuteMethod = nameof(CanOpenDefault))]
        private void OpenDefault()
        {
            OpenWithInnoSetupOrDefault(DefaultPath);
        }

        [CommandInvalidate(nameof(DefaultPath))]
        private bool CanOpenDefault() => !string.IsNullOrEmpty(DefaultPath);

        [Command(CanExecuteMethod = nameof(CanOpenTranslation))]
        private void OpenTranslation()
        {
            OpenWithInnoSetupOrDefault(TranslationPath);
        }

        [CommandInvalidate(nameof(TranslationPath))]
        private bool CanOpenTranslation() => !string.IsNullOrEmpty(TranslationPath);

        private void OpenWithInnoSetupOrDefault(string? filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show("File does not exist.", "Error while opening the file.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var compilerPaths = new[]
            {
                Path.Join(_compilerLocation, "ISIDE.exe"),
                Path.Join(_compilerLocation, "Compil32.exe"),
            };

            foreach (var compiler in compilerPaths)
            {
                if (!File.Exists(compiler))
                {
                    continue;
                }

                Process.Start(new ProcessStartInfo
                {
                    UseShellExecute = true,
                    Arguments = $"\"{filePath}\"",
                    FileName = compiler,
                });

                break;
            }
        }

        [CommandInvalidate(nameof(DefaultPath))]
        [CommandInvalidate(nameof(TranslationPath))]
        private bool CanLoad()
        {
            return !string.IsNullOrEmpty(_defaultPath)
                && !string.IsNullOrEmpty(_translationPath);
        }

        [Command(CanExecuteMethod = nameof(CanLoad))]
        private void Load()
        {
            if (DefaultPath is null || TranslationPath is null)
            {
                return;
            }

            Items.Clear();

            var linesDefault = File.ReadAllLines(DefaultPath, Encoding.UTF8);
            var linesTranslation = File.ReadAllLines(TranslationPath, Encoding.UTF8);

            IsTranslationRightToLeft = linesTranslation.Contains("RightToLeft=yes");

            foreach (var item in InnoSetupProvider.GetTranslationItems(linesDefault, linesTranslation))
            {
                Items.Add(item);
            }

            TranslationLastWriteTime = File.GetLastWriteTime(TranslationPath);

            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(TotalErrors));
            OnPropertyChanged(nameof(TranslationLastWriteTime));
        }
    }
}
