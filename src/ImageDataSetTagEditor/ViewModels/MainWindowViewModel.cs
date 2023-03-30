using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Alias;
using DynamicData.Binding;
using DynamicData.PLinq;
using ImageDataSetTagEditor.Views;
using ReactiveUI;

namespace ImageDataSetTagEditor.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly MainWindow _window;
    private static readonly string[] ValidImageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

    private readonly SourceCache<ImageViewModel, string> _images = new(x => x.ImagePath);
    private readonly SourceCache<GlobalTagViewModel, string> _globalTags = new(x => x.Tag.ToLowerInvariant());

    private ImageViewModel? _currentSelectedImage;
    private TagViewModel? _currentSelectedTag;
    private GlobalTagViewModel? _currentSelectedGlobalTag;
    private string _currentImageSearchTerm = string.Empty;
    private string _currentTagSearchTerm = string.Empty;
    private string? _currentSelectedSuggestion;
    private bool _isSaving;

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

    public string? CurrentSelectedSuggestion
    {
        get => _currentSelectedSuggestion;
        set => this.RaiseAndSetIfChanged(ref _currentSelectedSuggestion, value);
    }

    public bool IsSaving
    {
        get => _isSaving;
        set => this.RaiseAndSetIfChanged(ref _isSaving, value);
    }

    public string ImageCountText => $"{_images.Count} Images";
    public string TagCountText => $"{_globalTags.Count} Tags";

    public IObservableCollection<ImageViewModel> FilteredImages { get; } =
        new ObservableCollectionExtended<ImageViewModel>();

    public IObservableCollection<GlobalTagViewModel> FilteredTags { get; } =
        new ObservableCollectionExtended<GlobalTagViewModel>();

    public IObservableCollection<string> TagSuggestions { get; } = new ObservableCollectionExtended<string>();

    public ReactiveCommand<Unit, Unit> SelectedPreviousImageCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SelectedNextImageCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SaveAllCommand { get; set; }
    public ReactiveCommand<Unit, Unit> LoadImagesCommand { get; set; }
    public ReactiveCommand<Unit, Unit> AddTagCommand { get; set; }
    public ReactiveCommand<Unit, Unit> RemoveTagCommand { get; set; }
    public ReactiveCommand<Unit, TagViewModel?> EnterEditTagCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SelectNextTagCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SelectPreviousTagCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SetSuggestionCommand { get; set; }
    public ReactiveCommand<Unit, Unit> CloseSuggestionsCommand { get; set; }
    public ReactiveCommand<Unit, Unit> FocusSearchBoxCommand { get; set; } = ReactiveCommand.Create(() => { });
    public ReactiveCommand<Unit, Unit> FocusTagSearchBoxCommand { get; set; } = ReactiveCommand.Create(() => { });
    public ReactiveCommand<Unit, Unit> SelectNextImageWithGlobalTagCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SelectPreviousImageWithGlobalTagCommand { get; set; }
    public ReactiveCommand<Unit, Unit> ApplyCurrentGlobalTagToAllImagesCommand { get; set; }
    public ReactiveCommand<Unit, Unit> MoveTagUpCommand { get; set; }
    public ReactiveCommand<Unit, Unit> MoveTagDownCommand { get; set; }

    public MainWindowViewModel(MainWindow window)
    {
        _window = window;
        SelectedPreviousImageCommand = ReactiveCommand.Create(SelectedPreviousImage);
        SelectedNextImageCommand = ReactiveCommand.Create(SelectedNextImage);
        SaveAllCommand = ReactiveCommand.CreateFromTask(SaveAllAsync);
        LoadImagesCommand = ReactiveCommand.CreateFromTask(LoadImagesAsync);
        AddTagCommand = ReactiveCommand.Create(AddTag);
        RemoveTagCommand = ReactiveCommand.Create(RemoveTag);
        EnterEditTagCommand =
            ReactiveCommand.Create(() => CurrentSelectedTag = CurrentSelectedImage?.Tags.FirstOrDefault());
        SelectNextTagCommand = ReactiveCommand.Create(HandleSelectNextTag);
        SelectPreviousTagCommand = ReactiveCommand.Create(HandleSelectPreviousTag);
        SetSuggestionCommand = ReactiveCommand.Create(SetSuggestion);
        CloseSuggestionsCommand = ReactiveCommand.Create(CloseSuggestions);
        SelectNextImageWithGlobalTagCommand = ReactiveCommand.Create(SelectNextImageWithGlobalTag);
        SelectPreviousImageWithGlobalTagCommand = ReactiveCommand.Create(SelectPreviousImageWithGlobalTag);
        ApplyCurrentGlobalTagToAllImagesCommand = ReactiveCommand.Create(ApplyCurrentGlobalTagToAllImages);
        MoveTagUpCommand = ReactiveCommand.Create(() =>
        {
            if (CurrentSelectedTag is null) return;
            MoveTagUp(CurrentSelectedTag);
        });

        MoveTagDownCommand = ReactiveCommand.Create(() =>
        {
            if (CurrentSelectedTag is null) return;
            MoveTagDown(CurrentSelectedTag);
        });

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

        _globalTags.Connect()
            .Filter(FilterSuggestion, new ParallelisationOptions(ParallelType.Parallelise))
            .Sort(SortExpressionComparer<GlobalTagViewModel>.Ascending(x => x.Tag))
            .Select(x => x.Tag)
            .Bind(TagSuggestions)
            .Subscribe();
    }

    public void RefreshSuggestions() => _globalTags.Refresh();

    public void ApplyCurrentGlobalTagToAllImages()
    {
        if (CurrentSelectedGlobalTag is null) return;

        var images = _images.Items.Where(i => !i.Tags.Any(t =>
            t.Value.Equals(CurrentSelectedGlobalTag.Tag, StringComparison.InvariantCultureIgnoreCase)));
        foreach (var image in images)
        {
            var tag = new TagViewModel(CurrentSelectedGlobalTag.Tag);
            tag.OnValueChanged += RefreshSuggestions;

            image.Tags.Add(tag);
        }

        RebuildGlobalTags();
    }

    public void DeleteCurrentGlobalTagFromAllImages()
    {
        if (CurrentSelectedGlobalTag is null) return;

        var images = _images.Items.Where(i =>
                i.Tags.Any(t =>
                    t.Value.Equals(CurrentSelectedGlobalTag.Tag, StringComparison.InvariantCultureIgnoreCase)))
            .ToList();
        foreach (var image in images)
        {
            var tags = image.Tags.Where(x =>
                x.Value.Equals(CurrentSelectedGlobalTag.Tag, StringComparison.InvariantCultureIgnoreCase));
            image.Tags.RemoveMany(tags);
        }

        RebuildGlobalTags();
    }

    public void SelectNextImageWithGlobalTag()
    {
        if (CurrentSelectedGlobalTag is null) return;

        var images = FilteredImages.Where(i =>
                i.Tags.Any(t =>
                    t.Value.Equals(CurrentSelectedGlobalTag.Tag, StringComparison.InvariantCultureIgnoreCase)))
            .ToList();

        if (!images.Any())
            return;

        if (images.Count == 1 || CurrentSelectedImage is null)
            CurrentSelectedImage = images.FirstOrDefault();
        else
        {
            var index = images.IndexOf(CurrentSelectedImage);
            CurrentSelectedImage = index < images.Count - 1 ? images[index + 1] : images.FirstOrDefault();
        }
    }

    public void SelectPreviousImageWithGlobalTag()
    {
        if (CurrentSelectedGlobalTag is null) return;

        var images = FilteredImages.Where(i =>
                i.Tags.Any(t =>
                    t.Value.Equals(CurrentSelectedGlobalTag.Tag, StringComparison.InvariantCultureIgnoreCase)))
            .ToList();

        if (!images.Any())
            return;

        if (images.Count == 1 || CurrentSelectedImage is null)
            CurrentSelectedImage = images.LastOrDefault();
        else
        {
            var index = images.IndexOf(CurrentSelectedImage);
            CurrentSelectedImage = index > 0 ? images[index - 1] : images.LastOrDefault();
        }
    }

    public void MoveTagUp(TagViewModel tag)
    {
        if (CurrentSelectedImage is null) return;

        var index = CurrentSelectedImage.Tags.IndexOf(tag);
        if (index < 1) return;

        var temp = CurrentSelectedImage.Tags[index - 1];
        CurrentSelectedImage.Tags[index - 1] = tag;
        CurrentSelectedImage.Tags[index] = temp;

        CurrentSelectedTag = CurrentSelectedImage.Tags[index - 1];
    }

    public void MoveTagDown(TagViewModel tag)
    {
        if (CurrentSelectedImage is null) return;

        var index = CurrentSelectedImage.Tags.IndexOf(tag);
        if (index >= CurrentSelectedImage.Tags.Count - 1) return;

        var temp = CurrentSelectedImage.Tags[index + 1];
        CurrentSelectedImage.Tags[index + 1] = tag;
        CurrentSelectedImage.Tags[index] = temp;

        CurrentSelectedTag = CurrentSelectedImage.Tags[index + 1];
    }

    private void SetSuggestion()
    {
        if (CurrentSelectedTag is null || CurrentSelectedSuggestion is null ||
            !CurrentSelectedTag.ShowAutocomplete) return;

        if (CurrentSelectedTag.Value.Equals(CurrentSelectedSuggestion,
                StringComparison.InvariantCultureIgnoreCase)) return;

        CurrentSelectedTag.Value = CurrentSelectedSuggestion;
        RebuildGlobalTags();
    }

    private void CloseSuggestions()
    {
        if (CurrentSelectedTag is null || !CurrentSelectedTag.ShowAutocomplete) return;

        CurrentSelectedTag.ShowAutocomplete = false;
    }

    private void HandleSelectPreviousTag()
    {
        if (CurrentSelectedImage is null || CurrentSelectedTag is null) return;

        if (TagSuggestions.Count > 0 && CurrentSelectedTag.ShowAutocomplete && CurrentSelectedSuggestion is not null)
        {
            var index = TagSuggestions.IndexOf(CurrentSelectedSuggestion);
            CurrentSelectedSuggestion = index > 0 ? TagSuggestions[index - 1] : null;
        }
        else
            SelectPreviousTag();
    }

    private void SelectPreviousTag()
    {
        if (CurrentSelectedImage is null || CurrentSelectedTag is null) return;

        var index = CurrentSelectedImage.Tags.IndexOf(CurrentSelectedTag);
        CurrentSelectedTag = index <= 0
            ? CurrentSelectedImage.Tags.LastOrDefault()
            : CurrentSelectedImage.Tags[index - 1];
    }

    private void HandleSelectNextTag()
    {
        if (CurrentSelectedImage is null || CurrentSelectedTag is null) return;

        if (TagSuggestions.Count > 0 && CurrentSelectedTag.ShowAutocomplete)
        {
            if (CurrentSelectedSuggestion is null)
            {
                CurrentSelectedSuggestion = TagSuggestions.FirstOrDefault();
                return;
            }

            var index = TagSuggestions.IndexOf(CurrentSelectedSuggestion);
            if (index < TagSuggestions.Count - 1)
                CurrentSelectedSuggestion = TagSuggestions[index + 1];
            else
                SelectNextTag();
        }
        else
            SelectNextTag();
    }

    private void SelectNextTag()
    {
        if (CurrentSelectedImage is null || CurrentSelectedTag is null) return;

        var index = CurrentSelectedImage.Tags.IndexOf(CurrentSelectedTag);
        CurrentSelectedTag = index >= CurrentSelectedImage.Tags.Count - 1
            ? CurrentSelectedImage.Tags.FirstOrDefault()
            : CurrentSelectedImage.Tags[index + 1];
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

    private bool FilterSuggestion(GlobalTagViewModel currentGlobalTag)
    {
        return string.IsNullOrEmpty(CurrentSelectedTag?.Value) ||
               (currentGlobalTag.Tag.Contains(CurrentSelectedTag.Value, StringComparison.InvariantCultureIgnoreCase) &&
                !currentGlobalTag.Tag.Equals(CurrentSelectedTag.Value, StringComparison.InvariantCultureIgnoreCase));
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
                tag.OnValueChanged += _globalTags.Refresh;

                var global = _globalTags.Items.SingleOrDefault(globalTag =>
                                 string.Equals(globalTag.Tag, tag.Value,
                                     StringComparison.InvariantCultureIgnoreCase)) ??
                             new GlobalTagViewModel(tag.Value, 0);
                global.ImageCount++;
                _globalTags.AddOrUpdate(global);
            }

            return image;
        }).ToList();

        await Task.WhenAll(loadTasks);

        var loadedImages = loadTasks.Select(x => x.Result);
        _images.AddOrUpdate(loadedImages);

        CurrentSelectedImage = FilteredImages.FirstOrDefault();
        CurrentSelectedGlobalTag = FilteredTags.FirstOrDefault();

        this.RaisePropertyChanged(nameof(ImageCountText));
        this.RaisePropertyChanged(nameof(TagCountText));
    }

    public void RebuildGlobalTags()
    {
        var tags = _images.Items.SelectMany(x => x.Tags, (image, tag) => new { Iamge = image, Tag = tag })
            .GroupBy(x => x.Tag.Value, x => x.Iamge)
            .ToList();

        _globalTags.Clear();
        
        _globalTags.AddOrUpdate(tags.Select(tag => new GlobalTagViewModel(tag.Key, 0)
        {
            ImageCount = tag.Count()
        }));

        _globalTags.Refresh();
        this.RaisePropertyChanged(nameof(TagCountText));
    }

    private async Task SaveAllAsync()
    {
        IsSaving = true;
        var saveTasks = _images.Items.Select(async x => await x.SaveAsync());
        await Task.WhenAll(saveTasks);
        IsSaving = false;
    }

    private void AddTag()
    {
        if (CurrentSelectedImage is null) return;

        var newTag = new TagViewModel("New Tag");
        newTag.OnValueChanged += RefreshSuggestions;
        CurrentSelectedImage.Tags.Add(newTag);
        CurrentSelectedTag = newTag;
        
        _window.ScrollViewer.ScrollToEnd();
        
        RebuildGlobalTags();
    }

    private void RemoveTag()
    {
        if (CurrentSelectedImage is null || CurrentSelectedTag is null) return;

        CurrentSelectedTag.ShowAutocomplete = false;
        var image = CurrentSelectedImage;
        var index = image.Tags.IndexOf(CurrentSelectedTag);

        var global = _globalTags.Items.SingleOrDefault(x =>
            string.Equals(x.Tag, CurrentSelectedTag.Value, StringComparison.InvariantCultureIgnoreCase));

        image.Tags.Remove(CurrentSelectedTag);
        if (global is not null)
            if (global.ImageCount == 1)
                _globalTags.Remove(global);
            else
                global.ImageCount--;

        if (!image.Tags.Any()) return;

        CurrentSelectedTag = image.Tags[index == image.Tags.Count ? index - 1 : index];
        this.RaisePropertyChanged(nameof(TagCountText));
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