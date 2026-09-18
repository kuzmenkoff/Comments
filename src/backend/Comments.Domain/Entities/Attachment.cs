using Comments.Domain.Enums;

namespace Comments.Domain.Entities;

/// <summary>
/// A file attached to a comment: an image (JPG/GIF/PNG, resized to 320x240) or a text file (TXT up to 100 KB).
/// </summary>
public class Attachment
{
    public long Id { get; set; }

    public long CommentId { get; set; }
    public Comment Comment { get; set; } = null!;

    public AttachmentType Type { get; set; }
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
    /// <summary>Raw file bytes stored directly in the database.</summary>
    public byte[] Content { get; set; } = null!;
}
