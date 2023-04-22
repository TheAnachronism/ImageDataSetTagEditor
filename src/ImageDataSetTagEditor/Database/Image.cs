using System;
using System.Collections.Generic;

namespace ImageDataSetTagEditor.Database;

public class Image
{
    public Guid Id { get; set; }
    public string RelativePath { get; set; } = string.Empty;
    public List<Tag> Tags { get; set; } = new();
}