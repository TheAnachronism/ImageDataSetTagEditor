namespace ImageDataSetTagEditor.Models;

public class ImageTag
{
    private string _value;
    public string Value => _value;

    public ImageTag(string value)
    {
        _value = value;
    }
}