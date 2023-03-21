using ReactiveUI;

namespace ImageDataSetTagEditor.ViewModels;

public class TagViewModel : ReactiveObject
{
    private string _value;

    public string Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }

    public TagViewModel(string value)
    {
        _value = value;
    }
}