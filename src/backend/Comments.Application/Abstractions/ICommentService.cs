using Comments.Application.Common;
using Comments.Application.Dtos;

namespace Comments.Application.Abstractions;

/// <summary>Application use-cases for reading comments.</summary>
public interface ICommentService
{
    /// <summary>
    /// Returns a page of top-level comments with their full reply trees, sorted and paginated.
    /// </summary>
    Task<PagedResult<CommentDto>> GetPageAsync(
        int page,
        CommentSortField sortField,
        SortDirection direction,
        CancellationToken ct = default);

    /// <summary>
    /// Creates a new top-level comment or reply.
    /// </summary>
    Task<CommentDto> CreateAsync(CreateCommentRequest request, CancellationToken ct = default);

    /// <summary>Returns an attachment's raw bytes for download / lightbox, or null if not found.</summary>
    Task<AttachmentContent?> GetAttachmentAsync(long attachmentId, CancellationToken ct = default);

    /// <summary>Sanitizes text and returns the safe HTML that would be stored - without saving.</summary>
    string Preview(string text);
}
