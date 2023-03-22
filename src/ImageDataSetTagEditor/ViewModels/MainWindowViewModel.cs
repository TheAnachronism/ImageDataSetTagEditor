using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using DynamicData.PLinq;
using ReactiveUI;

namespace ImageDataSetTagEditor.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private static readonly string[] ValidImageTypes = { ".jpg", ".jpeg", ".png", ".gif" };
    private ImageViewModel? _currentSelectedImage;
    private TagViewModel? _currentSelectedTag;
    private string _currentSearchTerm = string.Empty;

    public string CountText => $"{_images.Count} Images loaded";

    private readonly SourceCache<ImageViewModel, string> _images = new(x => x.ImagePath);

    public IObservableCollection<ImageViewModel> FilteredImages { get; } =
        new ObservableCollectionExtended<ImageViewModel>();

    public string CurrentSearchTerm
    {
        get => _currentSearchTerm;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentSearchTerm, value);
            _images.Refresh();
        }
    }

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
    public ReactiveCommand<Unit, Unit> FocusSearchBoxCommand { get; set; } = ReactiveCommand.Create(() => { });

    public MainWindowViewModel()
    {
        _images.Connect()
            .Filter(Filter, new ParallelisationOptions(ParallelType.Ordered))
            .Sort(SortExpressionComparer<ImageViewModel>.Ascending(x => x.ImagePath))
            .Bind(FilteredImages)
            .Subscribe();

        LoadImagesCommand = ReactiveCommand.CreateFromTask(LoadImagesAsync);
        SaveAllCommand = ReactiveCommand.CreateFromTask(SaveAllAsync);
        AddTagCommand = ReactiveCommand.Create(AddTag);
        RemoveTagCommand = ReactiveCommand.Create(RemoveTag);
        SelectNextImageCommand = ReactiveCommand.Create(SelectNextImage);
        SelectPreviousImageCommand = ReactiveCommand.Create(SelectPreviousImage);
        EnterTagEditCommand = ReactiveCommand.Create(EnterTagEdit);
    }

    private bool Filter(ImageViewModel arg)
    {
        if (string.IsNullOrEmpty(_currentSearchTerm)) return true;

        var terms = _currentSearchTerm.Split(" ").Where(x => !string.IsNullOrEmpty(x));
        return terms.All(x => arg.ImageName.Contains(x, StringComparison.InvariantCultureIgnoreCase));
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

        _images.Clear();

        var files = Directory.EnumerateFiles(selectedDirectory, "*.*", SearchOption.AllDirectories)
            .Where(file => ValidImageTypes.Contains(Path.GetExtension(file)));

        var loadTasks = files.Select(async x =>
        {
            var image = new ImageViewModel(x, selectedDirectory);
            await image.LoadTagsAsync();
            return image;
        }).ToList();

        await Task.WhenAll(loadTasks);

        _images.AddOrUpdate(loadTasks.Select(x => x.Result));
        CurrentSelectedImage = FilteredImages.First();
        CurrentSelectedTag = CurrentSelectedImage.Tags.FirstOrDefault();
        this.RaisePropertyChanged(nameof(CountText));
    }

    private async Task SaveAllAsync()
    {
        foreach (var image in _images.Items) await image.SaveAsync();
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
        if (CurrentSelectedImage is null)
        {
            if (FilteredImages.Any())
                CurrentSelectedImage = FilteredImages.First();

            return;
        }

        var index = FilteredImages.IndexOf(CurrentSelectedImage);
        if (index < FilteredImages.Count - 1)
            CurrentSelectedImage = FilteredImages[index + 1];
    }

    private void SelectPreviousImage()
    {
        if (CurrentSelectedImage is null) return;

        var index = FilteredImages.IndexOf(CurrentSelectedImage);
        if (index > 0)
            CurrentSelectedImage = FilteredImages[index - 1];
    }
}