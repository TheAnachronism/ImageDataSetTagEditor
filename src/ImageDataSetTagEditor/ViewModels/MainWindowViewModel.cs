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
    private static readonly string[] ValidImageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

    private readonly SourceCache<ImageViewModel, string> _images = new(x => x.ImagePath);
    private readonly SourceCache<GlobalTagViewModel, string> _globalTags = new(x => x.Tag);

    private ImageViewModel? _currentSelectedImage;
    private TagViewModel? _currentSelectedTag;
    private GlobalTagViewModel? _currentSelectedGlobalTag;
    private string _currentImageSearchTerm = string.Empty;
    private string _currentTagSearchTerm = string.Empty;

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

    public GlobalTagViewModel? CurrentSelectedGlobalTag
    {
        get => _currentSelectedGlobalTag;
        set => this.RaiseAndSetIfChanged(ref _currentSelectedGlobalTag, value);
    }

    public string CurrentImageSearchTerm
    {
        get => _currentImageSearchTerm;
        set => this.RaiseAndSetIfChanged(ref _currentImageSearchTerm, value);
    }

    public string CurrentTagSearchTerm
    {
        get => _currentTagSearchTerm;
        set => this.RaiseAndSetIfChanged(ref _currentTagSearchTerm, value);
    }

    public string ImageCountText => $"{_images.Count} Images";
    public string TagCountText => $"{_globalTags.Count} Images";

    public IObservableCollection<ImageViewModel> FilteredImages { get; } =
        new ObservableCollectionExtended<ImageViewModel>();

    public IObservableCollection<GlobalTagViewModel> FilteredTags { get; } =
        new ObservableCollectionExtended<GlobalTagViewModel>();

    public IObservableCollection<string> TagSuggestions { get; } = new ObservableCollectionExtended<string>();

    public ReactiveCommand<Unit, Unit> SelectedPreviousImageCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SelectedNextImageCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SaveAllCommand { get; set; }
    public ReactiveCommand<Unit, Unit> LoadImagesCommand { get; set; }
    
    public MainWindowViewModel()
    {
        SelectedPreviousImageCommand = ReactiveCommand.Create(SelectedPreviousImage);
        SelectedNextImageCommand = ReactiveCommand.Create(SelectedNextImage);
        SaveAllCommand = ReactiveCommand.CreateFromTask(SaveAllAsync);
        LoadImagesCommand = ReactiveCommand.CreateFromTask(LoadImagesAsync);
        
        _images.Connect()
            .Filter(ImageFilter, new ParallelisationOptions(ParallelType.Parallelise))
            .Sort(SortExpressionComparer<ImageViewModel>.Ascending(x => x.ImagePath))
            .Bind(FilteredImages)
            .Subscribe();

        _globalTags.Connect()
            .Filter(GlobalTagFilter, new ParallelisationOptions(ParallelType.Parallelise))
            .Sort(SortExpressionComparer<GlobalTagViewModel>.Descending(x => x.ImageCount).ThenByAscending(x => x.Tag))
            .Bind(FilteredTags)
            .Subscribe();
    }

    private bool ImageFilter(ImageViewModel currentImage)
    {
        if (string.IsNullOrEmpty(_currentImageSearchTerm)) return true;

        var terms = _currentImageSearchTerm.Split(" ").Where(x => !string.IsNullOrEmpty(x));
        return terms.All(x => currentImage.Name.Contains(x, StringComparison.InvariantCultureIgnoreCase));
    }

    private bool GlobalTagFilter(GlobalTagViewModel currentGlobalTag)
    {
        if (string.IsNullOrEmpty(_currentTagSearchTerm)) return true;

        var terms = _currentTagSearchTerm.Split(" ").Where(x => !string.IsNullOrEmpty(x));
        return terms.All(x => currentGlobalTag.Tag.Contains(x, StringComparison.InvariantCultureIgnoreCase));
    }

    private async Task LoadImagesAsync()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Choose dataset root directory"
        };

        var selectedDirectory = await dialog.ShowAsync(new Window());
        if (selectedDirectory is null)
            return;

        _images.Clear();
        _globalTags.Clear();

        var files = Directory.EnumerateFiles(selectedDirectory, "*.*", SearchOption.AllDirectories)
            .Where(file => ValidImageExtensions.Contains(Path.GetExtension(file)));

        var loadTasks = files.Select(async x =>
        {
            var image = new ImageViewModel(x, selectedDirectory);
            var tags = await image.LoadAsync();

            foreach (var tag in tags)
            {
                var global = _globalTags.Items.SingleOrDefault(x =>
                                 string.Equals(x.Tag, tag, StringComparison.InvariantCultureIgnoreCase)) ??
                             new GlobalTagViewModel(tag, 0);
                global.ImageCount++;
                _globalTags.AddOrUpdate(global);
            }

            return image;
        }).ToList();

        await Task.WhenAll(loadTasks);

        var loadedImages = loadTasks.Select(x => x.Result);
        _images.AddOrUpdate(loadedImages);

        CurrentSelectedImage = FilteredImages.First();
        CurrentSelectedGlobalTag = FilteredTags.First();

        this.RaisePropertyChanged(nameof(ImageCountText));
        this.RaisePropertyChanged(nameof(TagCountText));
    }

    private async Task SaveAllAsync()
    {
        foreach (var image in _images.Items) await image.SaveAsync();
    }

    private void RemoveTag()
    {
        if (CurrentSelectedImage is null || CurrentSelectedTag is null) return;

        var image = CurrentSelectedImage;
        var index = image.Tags.IndexOf(CurrentSelectedTag);

        image.Tags.Remove(CurrentSelectedTag);
        var global = _globalTags.Items.SingleOrDefault(x =>
            string.Equals(x.Tag, CurrentSelectedTag.Value, StringComparison.InvariantCultureIgnoreCase));

        if (global is not null)
            if (global.ImageCount == 1)
                _globalTags.Remove(global);
            else
                global.ImageCount--;

        if (!image.Tags.Any()) return;

        CurrentSelectedTag = image.Tags[index == image.Tags.Count ? index - 1 : index];
    }

    private void SelectedNextImage()
    {
        if (CurrentSelectedImage is null)
        {
            if (FilteredImages.Any())
                CurrentSelectedImage = FilteredImages.First();

            return;
        }

        var index = FilteredImages.IndexOf(CurrentSelectedImage);
        CurrentSelectedImage = index < FilteredImages.Count - 1 ? FilteredImages[index + 1] : FilteredImages.First();
    }

    private void SelectedPreviousImage()
    {
        if (CurrentSelectedImage is null) return;

        var index = FilteredImages.IndexOf(CurrentSelectedImage);
        
        CurrentSelectedImage = index > 0 ? FilteredImages[index - 1] : FilteredImages.Last();
    }
}