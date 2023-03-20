using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;
using ImageDataSetTagEditor.Models;

namespace ImageDataSetTagEditor.ViewModels;

public class ImageViewModel
{
    public ImageViewModel(DataSetImage sourceImage)
    {
        Tags.AddRange(sourceImage.Tags.Select(x => new TagViewModel(x)));
    }
    
    public ObservableCollection<TagViewModel> Tags { get; } = new();
}