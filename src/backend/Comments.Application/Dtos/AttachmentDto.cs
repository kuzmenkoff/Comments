using Comments.Domain.Enums;

namespace Comments.Application.Dtos;

/// <summary>Attachment metadata for display.</summary>
public record AttachmentDto(long Id, AttachmentType Type, string FileName, string ContentType, long SizeBytes);
