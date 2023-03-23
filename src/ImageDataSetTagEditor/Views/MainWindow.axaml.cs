using System.Diagnostics;
using System.Linq;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using ImageDataSetTagEditor.ViewModels;

namespace ImageDataSetTagEditor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    public MainWindow(object? dataContext) : this()
    {
        DataContext = dataContext;
        ViewModel?.FocusSearchBoxCommand.Subscribe(new AnonymousObserver<Unit>(_ => ImageSearchBox.Focus()));
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

        listBox.ScrollIntoView(listBox.ItemCount - 1);

        if (listBox.GetVisualDescendants()
                .SingleOrDefault(x => x is TextBox { DataContext: TagViewModel tag } && tag == tagViewModel)
            is not TextBox textBox) return;

        textBox.Focus();
    }

    private void GlobalTagElement_OnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (sender is not Border { DataContext: GlobalTagViewModel } border) return;

        var clickedTag = (GlobalTagViewModel)border.DataContext;

        var images = ViewModel!.FilteredImages.Where(x => x.Tags.Any(y => y.Value.Equals(clickedTag.Value))).ToList();
        if (!images.Any())
            return;

        if (images.Count == 1 || ViewModel.CurrentSelectedImage is null)
            ViewModel.CurrentSelectedImage = images.First();
        else
        {
            var index = images.IndexOf(ViewModel.CurrentSelectedImage);
            ViewModel.CurrentSelectedImage = index < images.Count - 1 ? images[index + 1] : images.First();
        }
    }
}