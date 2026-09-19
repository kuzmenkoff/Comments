using Comments.Application.Common;

namespace Comments.Application.Abstractions;

/// <summary>Validates and resizes images.</summary>
public interface IImageProcessor
{
    /// <summary>Resizes to fit within maxWidth×maxHeight (keeping aspect ratio) if larger. Throws if not a supported image.</summary>
    ProcessedImage ResizeToFit(byte[] content, int maxWidth, int maxHeight);
}
