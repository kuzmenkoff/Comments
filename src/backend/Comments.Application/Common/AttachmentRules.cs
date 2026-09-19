namespace Comments.Application.Common;

/// <summary>Attachment limits from the spec.</summary>
public static class AttachmentRules
{
    public const int MaxImageWidth = 320;
    public const int MaxImageHeight = 240;
    public const int MaxTextBytes = 100 * 1024; // 100 KB
}
