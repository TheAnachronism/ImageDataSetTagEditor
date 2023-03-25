using ReactiveUI;

namespace ImageDataSetTagEditor.ViewModels;

public class TagViewModel : ReactiveObject
{
    private string _value;
    private bool _showAutocomplete;

    public string Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }

    public bool ShowAutocomplete
    {
        get => _showAutocomplete;
        set => this.RaiseAndSetIfChanged(ref _showAutocomplete, value);
    }

    public TagViewModel(string value)
    {
        _value = value;
    }
}