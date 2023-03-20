using System.Collections.Generic;
using ImageDataSetTagEditor.Models;

namespace ImageDataSetTagEditor.Services;

public interface IDataSetService
{
    IEnumerable<DataSetImage> LoadDataSet(string rootPath);
}