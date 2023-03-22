namespace ImageDataSetTagEditor.ViewModels;

public class GlobalTagViewModel
{
    public string Value { get; }
    public string ImageCount { get; } 

    public GlobalTagViewModel(string value, int imageCount)
    {
        Value = value;
        ImageCount = $"{imageCount} images";
    }
}