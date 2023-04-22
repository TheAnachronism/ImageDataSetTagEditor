using System;
using System.ComponentModel;
using Avalonia.Controls;
using ImageDataSetTagEditor.Messages;
using ImageDataSetTagEditor.ViewModels;
using ReactiveUI;

namespace ImageDataSetTagEditor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

        MessageBus.Current.Listen<FocusTextBoxMessage>().Subscribe(FocusTextBox);
    }

    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;

    private void Window_OnClosing(object? sender, CancelEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void FocusTextBox(FocusTextBoxMessage message)
    {
        switch (message.Type)
        {
            case FocusTextBoxMessage.TextBoxType.ImageSearch:
                ImageSearchTextBox.Focus();
                return;
            case FocusTextBoxMessage.TextBoxType.TagSearch:
                GlobalTagSearchBox.Focus();
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}