namespace Comments.Application.Dtos;

/// <summary>Raw attachment payload for download.</summary>
public record AttachmentContent(byte[] Content, string ContentType, string FileName);
