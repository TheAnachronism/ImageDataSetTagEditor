using ReactiveUI;

namespace ImageDataSetTagEditor.ViewModels;

public class GlobalTagViewModel : ReactiveObject
{
    private string _tag;
    private int _imageCount;

    public string Tag
    {
        get => _tag;
        set => this.RaiseAndSetIfChanged(ref _tag, value);
    }

    public int ImageCount
    {
        get => _imageCount;
        set
        {
            this.RaiseAndSetIfChanged(ref _imageCount, value);
            this.RaisePropertyChanged(nameof(ImageCountText));
        }
    }

    public string ImageCountText => $"{ImageCount} images";

    public GlobalTagViewModel(string tag, int initialImageCount)
    {
        _tag = tag;
        _imageCount = initialImageCount;
    }
}