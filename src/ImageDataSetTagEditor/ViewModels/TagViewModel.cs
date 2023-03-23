using ReactiveUI;

namespace ImageDataSetTagEditor.ViewModels;

public class TagViewModel : ReactiveObject
{
    private string _value;
    private bool _suggestTags;

    public string Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }

    public bool SuggestTags
    {
        get => _suggestTags;
        set => this.RaiseAndSetIfChanged(ref _suggestTags, value);
    }

    public TagViewModel(string value)
    {
        _value = value;
    }
}