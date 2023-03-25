using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ImageDataSetTagEditor;

public class BitmapAssetValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var maxSize = 1024;

        if (int.TryParse(parameter?.ToString(), out var height)) maxSize = height;

        if (value is string path)
            return ImageConverter.GetBitmap(path, maxSize);

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}