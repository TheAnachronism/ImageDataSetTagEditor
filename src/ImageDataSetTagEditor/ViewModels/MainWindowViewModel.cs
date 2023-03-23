using System;
using System.Diagnostics;
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
    private GlobalTagViewModel? _selectedGlobalTag;
    private string _currentImageSearchTerm = string.Empty;
    private string _currentTagSearchTerm = string.Empty;

    public string CountText => $"{_images.Count} Images";
    public string TagCountText => $"{_globalTags.Count} distinct tags";

    private readonly SourceCache<ImageViewModel, string> _images = new(x => x.ImagePath);
    private readonly SourceCache<GlobalTagViewModel, string> _globalTags = new(x => x.Value);

    public IObservableCollection<ImageViewModel> FilteredImages { get; } =
        new ObservableCollectionExtended<ImageViewModel>();

    public IObservableCollection<GlobalTagViewModel> FilteredTags { get; } =
        new ObservableCollectionExtended<GlobalTagViewModel>();

    public string CurrentImageSearchTerm
    {
        get => _currentImageSearchTerm;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentImageSearchTerm, value);
            _images.Refresh();
        }
    }

    public string CurrentTagSearchTerm
    {
        get => _currentTagSearchTerm;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentTagSearchTerm, value);
            _globalTags.Refresh();
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

    public GlobalTagViewModel? SelectedGlobalTag
    {
        get => _selectedGlobalTag;
        set => this.RaiseAndSetIfChanged(ref _selectedGlobalTag, value);
    }

    public ReactiveCommand<Unit, Unit> LoadImagesCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveAllCommand { get; }
    public ReactiveCommand<Unit, Unit> AddTagCommand { get; set; }
    public ReactiveCommand<Unit, Unit> RemoveTagCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SelectNextImageCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SelectPreviousImageCommand { get; set; }
    public ReactiveCommand<Unit, Unit> EnterTagEditCommand { get; set; }
    public ReactiveCommand<Unit, Unit> FocusSearchBoxCommand { get; set; } = ReactiveCommand.Create(() => { });
    public ReactiveCommand<Unit, Unit> RefreshGlobalTagsCommand { get; set; }

    public MainWindowViewModel()
    {
        _images.Connect()
            .Filter(FilterImages, new ParallelisationOptions(ParallelType.Ordered))
            .Sort(SortExpressionComparer<ImageViewModel>.Ascending(x => x.ImagePath))
            .Bind(FilteredImages)
            .Subscribe();

        _globalTags.Connect()
            .Filter(FilterTags, new ParallelisationOptions(ParallelType.Ordered))
            .Sort(SortExpressionComparer<GlobalTagViewModel>.Descending(x => x.ImageCount)
                .ThenByAscending(x => x.Value))
            .Bind(FilteredTags)
            .Subscribe();

        LoadImagesCommand = ReactiveCommand.CreateFromTask(LoadImagesAsync);
        SaveAllCommand = ReactiveCommand.CreateFromTask(SaveAllAsync);
        AddTagCommand = ReactiveCommand.Create(AddTag);
        RemoveTagCommand = ReactiveCommand.Create(RemoveTag);
        SelectNextImageCommand = ReactiveCommand.Create(SelectNextImage);
        SelectPreviousImageCommand = ReactiveCommand.Create(SelectPreviousImage);
        EnterTagEditCommand = ReactiveCommand.Create(EnterTagEdit);
        RefreshGlobalTagsCommand = ReactiveCommand.Create(RefreshGlobalTags);
    }

    private bool FilterImages(ImageViewModel image)
    {
        if (string.IsNullOrEmpty(_currentImageSearchTerm)) return true;

        var terms = _currentImageSearchTerm.Split(" ").Where(x => !string.IsNullOrEmpty(x));
        return terms.All(x => image.ImageName.Contains(x, StringComparison.InvariantCultureIgnoreCase));
    }

    private bool FilterTags(GlobalTagViewModel tag)
    {
        if (string.IsNullOrEmpty(_currentTagSearchTerm)) return true;

        var terms = _currentTagSearchTerm.Split(" ").Where(x => !string.IsNullOrEmpty(x));
        return terms.All(x => tag.Value.Contains(x, StringComparison.InvariantCultureIgnoreCase));
    }

    private void EnterTagEdit()
    {
        if (CurrentSelectedImage is null) return;
        CurrentSelectedTag = null;
        CurrentSelectedTag = CurrentSelectedImage.Tags.FirstOrDefault();
    }

    private void RefreshGlobalTags()
    {
        var tags = _images.Items.SelectMany(x => x.Tags, (image, tag) => new { Image = image, Tag = tag })
            .GroupBy(x => x.Tag.Value, x => x.Image)
            .ToList();

        _globalTags.Clear();
        _globalTags.AddOrUpdate(tags.Select(x => new GlobalTagViewModel(x.Key, x.Count())));
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

        var files = Directory.EnumerateFiles(selectedDirectory, "*.*", SearchOption.AllDirectories)
            .Where(file => ValidImageTypes.Contains(Path.GetExtension(file)));

        var loadTasks = files.Select(async x =>
        {
            var image = new ImageViewModel(x, selectedDirectory);
            await image.LoadTagsAsync();
            return image;
        }).ToList();

        await Task.WhenAll(loadTasks);

        var loadedImages = loadTasks.Select(x => x.Result).ToList();
        _images.AddOrUpdate(loadedImages);
        var tags = loadedImages.SelectMany(x => x.Tags, (image, tag) => new { Image = image, Tag = tag })
            .GroupBy(x => x.Tag.Value, x => x.Image)
            .ToList();

        _globalTags.Clear();
        _globalTags.AddOrUpdate(tags.Select(x => new GlobalTagViewModel(x.Key, x.Count())));

        CurrentSelectedImage = FilteredImages.First();
        CurrentSelectedTag = CurrentSelectedImage.Tags.FirstOrDefault();

        this.RaisePropertyChanged(nameof(CountText));
        this.RaisePropertyChanged(nameof(TagCountText));
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