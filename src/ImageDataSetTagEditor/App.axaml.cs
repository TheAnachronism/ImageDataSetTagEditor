using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ImageDataSetTagEditor.Services;
using ImageDataSetTagEditor.ViewModels;
using ImageDataSetTagEditor.Views;
using Splat;

namespace ImageDataSetTagEditor;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(Locator.GetLocator().GetService<IDataSetService>()!)
            };

        base.OnFrameworkInitializationCompleted();
    }
}