using System;
using System.ComponentModel;
using ReactiveUI;

namespace ImageDataSetTagEditor.ViewModels;

public class TagViewModel : ReactiveObject
{
    private string _value;
    private bool _showAutocomplete;

    public event Action? OnValueChanged;
    
    public string Value
    {
        get => _value;
        set
        {
            this.RaiseAndSetIfChanged(ref _value, value);
            OnOnValueChanged();
        }
    }

    public bool ShowAutocomplete
    {
        get => _showAutocomplete;
        set => this.RaiseAndSetIfChanged(ref _showAutocomplete, value);
    }

    [DesignOnly(true)]
    public TagViewModel()
    {
    }
    public TagViewModel(string value)
    {
        _value = value;
    }

    protected virtual void OnOnValueChanged()
    {
        OnValueChanged?.Invoke();
    }
}