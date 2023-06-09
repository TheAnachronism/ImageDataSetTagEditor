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
    public string Name => Path.GetRelativePath(_rootDirectory, ImagePath);
    public ObservableCollection<TagViewModel> Tags { get; set; } = new();
    private HashSet<string> _originalTags = new();

    public ImageViewModel(string imagePath, string rootDirectory)
    {
        _rootDirectory = rootDirectory;
        ImagePath = imagePath;
    }
    
    public async Task SaveAsync()
    {
        if (_originalTags.SetEquals(Tags.Select(x => x.Value).ToHashSet()))
            return;

        var tags = Tags.Select(x => x.Value).Where(x => !string.IsNullOrWhiteSpace(x));
        await File.WriteAllTextAsync(Path.ChangeExtension(ImagePath, ".txt"), string.Join(", ", tags));
    }

    public async Task<IEnumerable<TagViewModel>> LoadAsync()
    {
        var tagPath = Path.ChangeExtension(ImagePath, ".txt");
        if (!File.Exists(tagPath))
            return new List<TagViewModel>();

        var tags = (await File.ReadAllTextAsync(tagPath)).Split(", ").Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        
        _originalTags = tags.ToHashSet();
        Tags = new ObservableCollection<TagViewModel>(tags.Select(x => new TagViewModel(x)));

        return Tags;
    }
}