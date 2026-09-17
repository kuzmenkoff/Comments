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
}
