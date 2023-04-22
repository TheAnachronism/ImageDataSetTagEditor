using System;

namespace ImageDataSetTagEditor.Database;

public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Image Image { get; set; } = null!;
}