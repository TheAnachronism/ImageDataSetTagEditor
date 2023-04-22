using System;
using System.Reactive;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using ImageDataSetTagEditor.Messages;
using ReactiveUI;

namespace ImageDataSetTagEditor.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private static readonly string[] ValidImageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

    private string _imageSearchTerm = string.Empty;
    private string _globalTagSearchTerm = string.Empty;

    private ImageViewModel? _selectedImage;

    private readonly SourceCache<ImageViewModel, string> _images = new(x => x.ImagePath);

    public string ImageCountText => $"NUMBER Images";
    public string GlobalTagCountText => $"NUMBER Tags";

    public string ImageSearchTerm
    {
        get => _imageSearchTerm;
        set => this.RaiseAndSetIfChanged(ref _imageSearchTerm, value);
    }

    public string GlobalTagSearchTerm
    {
        get => _globalTagSearchTerm;
        set => this.RaiseAndSetIfChanged(ref _globalTagSearchTerm, value);
    }

    public ImageViewModel? SelectedImage
    {
        get => _selectedImage;
        set => this.RaiseAndSetIfChanged(ref _selectedImage, value);
    }

    public IObservableCollection<ImageViewModel> FilteredImages { get; } =
        new ObservableCollectionExtended<ImageViewModel>();

    public ReactiveCommand<Unit, Unit> LoadImagesCommand { get; set; }
    public ReactiveCommand<Unit, Unit> FocusImageSearchCommand { get; set; }
    public ReactiveCommand<Unit, Unit> FocusTagSearchCommand { get; set; }

    public MainWindowViewModel()
    {
        LoadImagesCommand = ReactiveCommand.CreateFromTask(LoadImagesAsync);
        FocusImageSearchCommand = ReactiveCommand.Create(FocusImageSearch);
        FocusTagSearchCommand = ReactiveCommand.Create(FocusTagSearch);
    }

    private async Task LoadImagesAsync()
    {
        throw new NotImplementedException();
    }

    private void FocusImageSearch() =>
        MessageBus.Current.SendMessage(new FocusTextBoxMessage(FocusTextBoxMessage.TextBoxType.ImageSearch, this));

    private void FocusTagSearch() =>
        MessageBus.Current.SendMessage(new FocusTextBoxMessage(FocusTextBoxMessage.TextBoxType.TagSearch, this));
}