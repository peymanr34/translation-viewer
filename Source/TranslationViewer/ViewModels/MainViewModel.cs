using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using MvvmGen;
using TranslationViewer.Models;
using TranslationViewer.Utilities;

namespace TranslationViewer.ViewModels
{
    [ViewModel]
    public partial class MainViewModel
    {
        partial void OnInitialize()
        {
            Items = new ObservableCollection<TranslationItem>();
        }

        [Property]
        private string? _defaultPath;

        [Property]
        private string? _translationPath;

        [Property]
        private bool _isTranslationRightToLeft;

        [Property]
        private ObservableCollection<TranslationItem> _items;

        public int TotalCount
            => _items?.Count ?? 0;

        public int TotalErrors
            => _items?.Count(x => x.HasErrors) ?? 0;

        public DateTime? TranslationLastWriteTime { get; private set; }

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
