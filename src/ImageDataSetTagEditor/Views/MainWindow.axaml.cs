using System.Threading.Tasks;
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

    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;
}