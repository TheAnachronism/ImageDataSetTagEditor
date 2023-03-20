using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using DynamicData;
using ImageDataSetTagEditor.Services;
using ReactiveUI;
using Splat;

namespace ImageDataSetTagEditor.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IDataSetService _dataSetService;

    public MainWindowViewModel()
    {
        _dataSetService = new DataSetService();
    }

    [DependencyInjectionConstructor]
    public MainWindowViewModel(IDataSetService dataSetService)
    {
        _dataSetService = dataSetService;

        LoadImagesCommand = ReactiveCommand.CreateFromTask(LoadImagesAsync);
        AddTagCommand = ReactiveCommand.Create(AddTag);
    }

    public ReactiveCommand<Unit, Unit> LoadImagesCommand { get; }
    public ReactiveCommand<Unit, Unit> AddTagCommand { get; }
    public ObservableCollection<ImageViewModel> Images { get; } = new();
    
    private void AddTag()
    {
        throw new NotImplementedException();
    }

    private async Task LoadImagesAsync()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Choose dataset root directory."
        };

        var selectedDirectory = await dialog.ShowAsync(new Window());
        if (selectedDirectory is null)
            return;

        var images = _dataSetService.LoadDataSet(selectedDirectory);
        Images.AddRange(images.Select(x => new ImageViewModel(x)));
    }
}