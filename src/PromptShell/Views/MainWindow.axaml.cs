using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PromptShell.ViewModels;

namespace PromptShell.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Event handler for the folder browser button click event.
    /// </summary>
    private async void BrowseFolderButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Active Working Directory",
                AllowMultiple = false
            });

            if (folders.Count > 0)
            {
                viewModel.CurrentWorkingDirectory = folders[0].Path.LocalPath;
            }
        }
    }
}