namespace Comments.Application.Dtos;

/// <summary>
/// A framework-agnostic uploaded file. The API layer maps IFormFile into this.
/// </summary>
public record UploadedFile(string FileName, string ContentType, byte[] Content);
