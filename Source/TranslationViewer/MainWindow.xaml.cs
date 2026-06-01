using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
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
            _viewModel.Initialize();

            DataContext = _viewModel;

            Title = $"Translation Viewer (v{Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString(3)})";
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
