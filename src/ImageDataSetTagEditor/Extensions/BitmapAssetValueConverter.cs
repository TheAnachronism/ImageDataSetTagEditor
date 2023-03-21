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
        
        if (int.TryParse(parameter?.ToString(), out var height))
        {
            maxSize = height;
        }
        
        if (value is string path)
        {
            using var origImage = SKBitmap.Decode(path);

            int targetWidth, targetHeight;
            if (origImage.Width > origImage.Height)
            {
                targetWidth = Math.Min(origImage.Width, maxSize);
                targetHeight = (int)(origImage.Height * (float)targetWidth / origImage.Width);
            }
            else
            {
                targetHeight = Math.Min(origImage.Height, maxSize);
                targetWidth = (int)(origImage.Width * (float)targetHeight / origImage.Height);
            }

            using var resizedImage =
                origImage.Resize(new SKImageInfo(targetWidth, targetHeight), SKFilterQuality.Medium);

            using var data = resizedImage.Encode(SKEncodedImageFormat.Jpeg, 100);
            using var stream = new MemoryStream(data.ToArray());
            return new Bitmap(stream);
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}