using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using TranslationViewer.Utilities;
using TranslationViewer.ViewModels;

namespace TranslationViewer
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            _viewModel.DefaultPath ??= InnoSetupProvider.GetDefaultTranslationFile();

            DataContext = _viewModel;
        }

        private static void OpenWithInnoSetupOrDefault(string? fileName)
        {
            if (!File.Exists(fileName))
            {
                MessageBox.Show("File doesn't exists.");
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                Arguments = $"\"{fileName}\"",
                FileName = InnoSetupProvider.GetExecutableLocation(),
            });
        }

        private void OpenDefaultFile_Click(object sender, RoutedEventArgs e)
        {
            OpenWithInnoSetupOrDefault(_viewModel.DefaultPath);
        }

        private void OpenTranslationFile_Click(object sender, RoutedEventArgs e)
        {
            OpenWithInnoSetupOrDefault(_viewModel.TranslationPath);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            versionTextBlock.Text = Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_viewModel.TranslationPath))
            {
                return;
            }

            var cached = _viewModel.TranslationLastWriteTime;
            var current = File.GetLastWriteTime(_viewModel.TranslationPath);

            if (cached < current)
            {
                int index = listView1.SelectedIndex;

                Debug.WriteLine($"Performing reload. SelectedIndex: {index}");

                _viewModel.LoadCommand.Execute(null);

                if (listView1.Items.Count > index)
                {
                    listView1.SelectedIndex = index;
                }
            }
        }
    }
}
