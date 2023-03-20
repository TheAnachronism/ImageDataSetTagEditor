using System.Collections.Generic;
using System.IO;

namespace ImageDataSetTagEditor.Models;

public class DataSetImage
{
    private string _path;
    
    private List<ImageTag> _tags = new();
    public IEnumerable<ImageTag> Tags => _tags;

    public string ImagePath => _path;
    public string ImageName => Path.GetFileNameWithoutExtension(_path);
    public DataSetImage(string path)
    {
        _path = path;
    }
}