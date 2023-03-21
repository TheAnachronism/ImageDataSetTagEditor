using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;

namespace ImageDataSetTagEditor.ViewModels;

public class ImageViewModel : ReactiveObject
{
    private readonly string _rootDirectory;
    public string ImagePath { get; }
    public string ImageName => Path.GetRelativePath(_rootDirectory, ImagePath);
    private HashSet<string> _originalTags = new();

    public ObservableCollection<TagViewModel> Tags { get; private set; } = new();

    public ImageViewModel(string imagePath, string rootDirectory)
    {
        ImagePath = imagePath;
        _rootDirectory = rootDirectory;
    }

    public async Task SaveAsync()
    {
        if (_originalTags.SetEquals(Tags.Select(x => x.Value).ToHashSet()))
            return;

        var tags = Tags.Select(x => x.Value);
        await File.WriteAllTextAsync(Path.ChangeExtension(ImagePath, ".txt"), string.Join(", ", tags));
    }

    public async Task LoadTagsAsync()
    {
        var path = Path.ChangeExtension(ImagePath, ".txt");
        if (!File.Exists(path))
            return;

        var tags = (await File.ReadAllTextAsync(path)).Split(", ").Where(x => !string.IsNullOrEmpty(x)).ToList();

        _originalTags = tags.ToHashSet();
        Tags = new ObservableCollection<TagViewModel>(tags.Select(x => new TagViewModel(x)));
    }
}