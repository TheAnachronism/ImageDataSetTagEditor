using Avalonia;
using Avalonia.ReactiveUI;
using System;
using ImageDataSetTagEditor.Services;
using ImageDataSetTagEditor.ViewModels;
using Splat;

namespace ImageDataSetTagEditor;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        SplatRegistrations.RegisterLazySingleton<IDataSetService, DataSetService>();
        SplatRegistrations.Register<MainWindowViewModel>();

        SplatRegistrations.SetupIOC();
        
        var app = BuildAvaloniaApp();

        app.StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
}