using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using DynamicData;
using ReactiveUI;

namespace ImageDataSetTagEditor.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private static readonly string[] ValidImageTypes = { ".jpg", ".jpeg", ".png", ".gif" };
    private ImageViewModel? _currentSelectedImage;
    private TagViewModel? _currentSelectedTag;

    public string CountText => $"{Images.Count} Images loaded";

    public ObservableCollection<ImageViewModel> Images { get; set; } = new();

    public ImageViewModel? CurrentSelectedImage
    {
        get => _currentSelectedImage;
        set => this.RaiseAndSetIfChanged(ref _currentSelectedImage, value);
    }

    public TagViewModel? CurrentSelectedTag
    {
        get => _currentSelectedTag;
        set => this.RaiseAndSetIfChanged(ref _currentSelectedTag, value);
    }

    public ReactiveCommand<Unit, Unit> LoadImagesCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveAllCommand { get; }
    public ReactiveCommand<Unit, Unit> AddTagCommand { get; set; }
    public ReactiveCommand<Unit, Unit> RemoveTagCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SelectNextImageCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SelectPreviousImageCommand { get; set; }
    public ReactiveCommand<Unit, Unit> EnterTagEditCommand { get; set; }

    public MainWindowViewModel()
    {
        LoadImagesCommand = ReactiveCommand.CreateFromTask(LoadImagesAsync);
        SaveAllCommand = ReactiveCommand.CreateFromTask(SaveAllAsync);
        AddTagCommand = ReactiveCommand.Create(AddTag);
        RemoveTagCommand = ReactiveCommand.Create(RemoveTag);
        SelectNextImageCommand = ReactiveCommand.Create(SelectNextImage);
        SelectPreviousImageCommand = ReactiveCommand.Create(SelectPreviousImage);
        EnterTagEditCommand = ReactiveCommand.Create(EnterTagEdit);
    }

    private void EnterTagEdit()
    {
        if (CurrentSelectedImage is null) return;
        CurrentSelectedTag = null;
        CurrentSelectedTag = CurrentSelectedImage.Tags.FirstOrDefault();
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

        Images.Clear();
        
        var files = Directory.EnumerateFiles(selectedDirectory, "*.*", SearchOption.AllDirectories)
            .Where(file => ValidImageTypes.Contains(Path.GetExtension(file)));

        var loadTasks = files.Select(async x =>
        {
            var image = new ImageViewModel(x, selectedDirectory);
            await image.LoadTagsAsync();
            return image;
        }).ToList();

        await Task.WhenAll(loadTasks);

        Images.AddRange(loadTasks.Select(x => x.Result));
        CurrentSelectedImage = Images.First();
        CurrentSelectedTag = CurrentSelectedImage.Tags.FirstOrDefault();
        this.RaisePropertyChanged(nameof(CountText));
    }

    private async Task SaveAllAsync()
    {
        foreach (var image in Images) await image.SaveAsync();
    }

    private void AddTag()
    {
        if (CurrentSelectedImage?.Tags.Any(x => string.IsNullOrEmpty(x.Value)) == true) return;
        
        CurrentSelectedImage?.Tags.Add(new TagViewModel(string.Empty));
        CurrentSelectedTag = CurrentSelectedImage?.Tags.Last();
    }

    private void RemoveTag()
    {
        if (CurrentSelectedImage is null || CurrentSelectedTag is null) return;

        var image = CurrentSelectedImage;
        var index = image.Tags.IndexOf(CurrentSelectedTag);

        image.Tags.Remove(CurrentSelectedTag);
        if (!image.Tags.Any()) return;
        
        CurrentSelectedTag = image.Tags[index == image.Tags.Count ? index - 1 : index];
    }

    private void SelectNextImage()
    {
        if (CurrentSelectedImage is null) return;

        var index = Images.IndexOf(CurrentSelectedImage);
        if (index < Images.Count - 1)
            CurrentSelectedImage = Images[index + 1];
    }

    private void SelectPreviousImage()
    {
        if (CurrentSelectedImage is null) return;

        var index = Images.IndexOf(CurrentSelectedImage);
        if (index > 0)
            CurrentSelectedImage = Images[index - 1];
    }
}