using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using DynamicData;
using ImageDataSetTagEditor.ViewModels;
using ReactiveUI;

namespace ImageDataSetTagEditor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(object? dataContest) : this()
    {
        DataContext = dataContest;

        ViewModel.FocusSearchBoxCommand.Subscribe(new AnonymousObserver<Unit>(_ => ImageSearchTextBox.Focus()));
        ViewModel.FocusTagSearchBoxCommand.Subscribe(new AnonymousObserver<Unit>(_ => GlobalTagSearchBox.Focus()));
    }

    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;

    private async void ImageListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var image = e.RemovedItems.Cast<ImageViewModel>().SingleOrDefault();
        if (image is null) return;

        await image.SaveAsync();
        var newImage = e.AddedItems.Cast<ImageViewModel>().SingleOrDefault();
        ViewModel.CurrentSelectedTag = newImage?.Tags.FirstOrDefault();
    }

    private void TagListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox tagList) return;
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is string) return;

        if (e.AddedItems.Cast<TagViewModel>().SingleOrDefault() is not { } tagViewModel) return;

        if (tagList.GetVisualDescendants()
                .SingleOrDefault(x => x is TextBox { DataContext: TagViewModel tag } && tag == tagViewModel) is not
            TextBox textBox) return;

        textBox.Focus();
    }

    private void GlobalTag_OnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        ViewModel.SelectNextImageWithGlobalTag();
    }

    private void Tag_OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is not TextBox { DataContext: TagViewModel tag }) return;
        if (ViewModel.CurrentSelectedTag != tag)
            ViewModel.CurrentSelectedTag = tag;

        ViewModel.CurrentSelectedTag.ShowAutocomplete = true;
        ViewModel.CurrentSelectedSuggestion = null;
        ViewModel.RefreshSuggestions();
    }

    private void Tag_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextBox { DataContext: TagViewModel tag }) return;

        if (tag.ShowAutocomplete)
            ViewModel.RebuildGlobalTags();

        tag.ShowAutocomplete = false;
    }

    private void Suggestion_OnTapped(object? sender, RoutedEventArgs _)
    {
        if (sender is TextBlock { DataContext: string suggestion })
            SetSuggestion(suggestion);
    }

    private void Suggestion_OnPointerPressed(object? sender, PointerPressedEventArgs _)
    {
        if (sender is TextBlock { DataContext: string suggestion })
            SetSuggestion(suggestion);
    }

    private void SetSuggestion(string suggestion)
    {
        if (ViewModel.CurrentSelectedTag is null) return;

        if (!ViewModel.CurrentSelectedTag.Value.Equals(suggestion, StringComparison.InvariantCultureIgnoreCase))
            ViewModel.CurrentSelectedTag.Value = suggestion;
    }

    private void SelectNextImageWithGlobalTag_OnClick(object? sender, RoutedEventArgs e) =>
        ViewModel.SelectNextImageWithGlobalTag();

    private void SelectPreviousImageWithGlobalTag_OnClick(object? sender, RoutedEventArgs e) =>
        ViewModel.SelectPreviousImageWithGlobalTag();

    private void ApplyGlobalTagToAllImages_OnClick(object? sender, RoutedEventArgs e) =>
        ViewModel.ApplyCurrentGlobalTagToAllImages();

    private async void Window_OnClosing(object? sender, CancelEventArgs e)
    {
        if (ViewModel.IsSaving)
            e.Cancel = true;
        else
            await ViewModel.SaveAllCommand.Execute();
    }

    private void DeleteGlobalTagFromAllImages_OnClick(object? sender, RoutedEventArgs e) =>
        ViewModel.DeleteCurrentGlobalTagFromAllImages();
}