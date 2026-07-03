using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PromptShell.ViewModels;
using System;
using System.Globalization;

namespace PromptShell.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        DataContextChanged += (sender, args) =>
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.CopyToClipboardAction = async (text) =>
                {
                    if (Clipboard != null)
                    {
                        await Clipboard.SetTextAsync(text);
                    }
                };
            }
        };
    }

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

/// <summary>
/// Compares a string value with a parameter and returns true if they match.
/// </summary>
public class StringComparisonConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null)
            return false;

        var valueStr = value.ToString();
        var paramStr = parameter.ToString();
        
        if (valueStr is null || paramStr is null)
            return false;

        return valueStr.Equals(paramStr, StringComparison.OrdinalIgnoreCase);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}