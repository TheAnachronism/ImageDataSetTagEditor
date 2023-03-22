using System;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using SkiaSharp;

namespace ImageDataSetTagEditor.Extensions;

public class BitmapConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        var maxSize = 1024;
        
        if (int.TryParse(parameter?.ToString(), out var height)) maxSize = height;

        if (value is string path)
            return ImageConverter.GetBitmap(path, maxSize);

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}