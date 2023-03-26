using Avalonia.Controls;
using Avalonia.Interactivity;
using ImageDataSetTagEditor.ViewModels;

namespace ImageDataSetTagEditor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    
    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;

    private void ImageListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // throw new System.NotImplementedException();
    }

    private void TagListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // throw new System.NotImplementedException();
    }

    private void GlobalTag_OnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        // throw new System.NotImplementedException();
    }
}