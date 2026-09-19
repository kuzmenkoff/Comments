using Comments.Application.Abstractions;
using Comments.Application.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Comments.Infrastructure.Services;

public class ImageSharpProcessor : IImageProcessor
{
    private static readonly HashSet<string> AllowedFormats =
        new(StringComparer.OrdinalIgnoreCase) { "JPEG", "PNG", "GIF" };

    public ProcessedImage ResizeToFit(byte[] content, int maxWidth, int maxHeight)
    {
        using var image = SixLabors.ImageSharp.Image.Load(content); // throws on non-image data
        var format = image.Metadata.DecodedImageFormat
            ?? throw new InvalidOperationException("Unknown image format.");

        if (!AllowedFormats.Contains(format.Name))
            throw new InvalidOperationException($"Unsupported image format: {format.Name}");

        // Only shrink; ResizeMode.Max keeps aspect ratio and fits within the box.
        if (image.Width > maxWidth || image.Height > maxHeight)
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(maxWidth, maxHeight),
                Mode = ResizeMode.Max
            }));

        using var ms = new MemoryStream();
        var encoder = image.Configuration.ImageFormatsManager.GetEncoder(format);
        image.Save(ms, encoder); // re-encode in the original format

        return new ProcessedImage(ms.ToArray(), format.DefaultMimeType, image.Width, image.Height);
    }
}
