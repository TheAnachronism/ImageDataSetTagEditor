using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using ImageDataSetTagEditor.ViewModels;

namespace ImageDataSetTagEditor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(object? dataContext) : this()
    {
        DataContext = dataContext;
    }

    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    private async void ImageList_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var image = e.RemovedItems.Cast<ImageViewModel>().SingleOrDefault();
        if (image is null) return;

        await image.SaveAsync();
        var newImage = e.AddedItems.Cast<ImageViewModel>().SingleOrDefault();
        ViewModel!.CurrentSelectedTag = newImage?.Tags.FirstOrDefault();
    }

    private void TagTextBox_OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is not TextBox { DataContext: TagViewModel tag }) return;
        if (ViewModel!.CurrentSelectedTag != tag)
            ViewModel!.CurrentSelectedTag = tag;
    }

    private void TagListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox listBox) return;
        if (e.AddedItems.Cast<TagViewModel>().SingleOrDefault() is not { } tagViewModel) return;

        if (listBox.GetVisualChildren()
                .SingleOrDefault(x => x is TextBox { DataContext: TagViewModel tag } && tag == tagViewModel)
            is TextBox textBox)
            textBox.Focus();
    }
}