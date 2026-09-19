namespace Comments.Application.Common;

/// <summary>Result of validating/resizing an image.</summary>
public record ProcessedImage(byte[] Content, string ContentType, int Width, int Height);
