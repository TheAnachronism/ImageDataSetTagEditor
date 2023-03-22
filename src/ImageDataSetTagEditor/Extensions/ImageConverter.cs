using System;
using System.IO;
using Avalonia.Media.Imaging;
using SkiaSharp;

namespace ImageDataSetTagEditor.Extensions;

public static class ImageConverter
{
    public static Bitmap? GetBitmap(string? path, int maxSize)
    {
        if (string.IsNullOrEmpty(path)) return null;
        
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
}