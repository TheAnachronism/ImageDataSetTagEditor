using System.Collections.ObjectModel;
using System.IO;
using ReactiveUI;

namespace ImageDataSetTagEditor.ViewModels;

public class ImageViewModel : ReactiveObject
{
    private readonly string _rootDirectory;
    public string ImagePath { get; }
    public string Name => Path.GetRelativePath(_rootDirectory, ImagePath);

    public ObservableCollection<TagViewModel> Tags { get; set; } = new();

    public ImageViewModel(string path, string rootDirectory)
    {
        _rootDirectory = rootDirectory;
        ImagePath = path;
    }
}