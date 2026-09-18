using Comments.Application.Common;
using Comments.Domain.Entities;

namespace Comments.Application.Abstractions;

/// <summary>Data access for comments and their attachments.</summary>
public interface ICommentRepository
{
    /// <summary>
    /// Returns one page of top-level comments (ParentId == null), sorted and paginated.
    /// Backs the sortable, paged table on the main page (25 per page, LIFO by default).
    /// </summary>
    Task<PagedResult<Comment>> GetTopLevelAsync(
        int page,
        int pageSize,
        CommentSortField sortField,
        SortDirection direction,
        CancellationToken ct = default);

    /// <summary>
    /// Returns all descendants (any depth) of the given root comments as a flat list.
    /// The service assembles them into a tree for cascading display.
    /// </summary>
    Task<IReadOnlyList<Comment>> GetDescendantsAsync(
        IReadOnlyCollection<long> rootIds,
        CancellationToken ct = default);

    /// <summary>Checks a parent exists before attaching a reply to it.</summary>
    Task<bool> ExistsAsync(long id, CancellationToken ct = default);

    /// <summary>Persists a new comment (and its attachment, if any) and returns it with its Id.</summary>
    Task<Comment> AddAsync(Comment comment, CancellationToken ct = default);

    /// <summary>Loads an attachment by id for file download / preview (lightbox).</summary>
    Task<Attachment?> GetAttachmentAsync(long attachmentId, CancellationToken ct = default);
}
