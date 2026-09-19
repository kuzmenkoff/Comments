using Comments.Application.Dtos;
using Comments.Domain.Entities;

namespace Comments.Application.Abstractions;

/// <summary>Turns an uploaded file into a validated Attachment (image or text).</summary>
public interface IAttachmentService
{
    Attachment Create(UploadedFile file);
}
