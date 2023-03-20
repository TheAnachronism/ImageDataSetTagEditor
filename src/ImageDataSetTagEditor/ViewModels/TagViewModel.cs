using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ImageDataSetTagEditor.Models;

namespace ImageDataSetTagEditor.ViewModels;

public class TagViewModel : INotifyPropertyChanged
{
    private string _value;

    public string Value
    {
        get => _value;
        set
        {
            if (value == _value) return;
            _value = value;
            OnPropertyChanged();
        }
    }

    public TagViewModel(ImageTag sourceTag)
    {
        _value = sourceTag.Value;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}