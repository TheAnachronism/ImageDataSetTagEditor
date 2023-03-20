using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageDataSetTagEditor.Models;

namespace ImageDataSetTagEditor.Services;

public class DataSetService : IDataSetService
{
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

    private readonly List<DataSetImage> _loadedImages = new();

    public IEnumerable<DataSetImage> LoadDataSet(string rootPath)
    {
        _loadedImages.Clear();
        var files = Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)
            .Where(file => ImageExtensions.Contains(Path.GetExtension(file)));

        _loadedImages.AddRange(files.Select(file => new DataSetImage(file)));

        return _loadedImages;
    }
}