using Comments.Application.Abstractions;
using Comments.Application.Common;
using Comments.Application.Dtos;
using Comments.Domain.Entities;

namespace Comments.Application.Services;

/// <summary>Reads comments and assembles flat rows into reply trees.</summary>
public class CommentService(ICommentRepository repository) : ICommentService
{
    public async Task<PagedResult<CommentDto>> GetPageAsync(
        int page,
        CommentSortField sortField,
        SortDirection direction,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;

        // 1. One page of root comments (ParentId == null), sorted + paginated.
        var roots = await repository.GetTopLevelAsync(
            page, PaginationDefaults.PageSize, sortField, direction, ct);

        if (roots.Items.Count == 0)
            return Empty(page);

        // 2. All descendants of those roots, any depth, in a single query.
        var rootIds = roots.Items.Select(c => c.Id).ToArray();
        var descendants = await repository.GetDescendantsAsync(rootIds, ct);

        // 3. Assemble trees in memory.
        var trees = BuildTrees(roots.Items, descendants);

        return new PagedResult<CommentDto>
        {
            Items = trees,
            Page = roots.Page,
            PageSize = roots.PageSize,
            TotalCount = roots.TotalCount
        };
    }

    /// <summary>Links flat comments into trees by ParentId; returns the roots in their given order.</summary>
    private static List<CommentDto> BuildTrees(
        IReadOnlyList<Comment> roots,
        IReadOnlyList<Comment> descendants)
    {
        // Map every entity to a DTO, indexed by id.
        var all = roots.Concat(descendants).ToList();
        var byId = all.ToDictionary(c => c.Id, MapToDto);

        // Attach each non-root to its parent's Replies.
        foreach (var comment in all)
        {
            if (comment.ParentId is { } parentId && byId.TryGetValue(parentId, out var parent))
                parent.Replies.Add(byId[comment.Id]);
        }

        // Replies read oldest-first (natural conversation flow).
        foreach (var dto in byId.Values)
            dto.Replies.Sort((a, b) => a.CreatedAt.CompareTo(b.CreatedAt));

        // Return roots preserving the sorted/paginated order from the repository.
        return roots.Select(r => byId[r.Id]).ToList();
    }

    private static CommentDto MapToDto(Comment c) => new()
    {
        Id = c.Id,
        ParentId = c.ParentId,
        UserName = c.UserName,
        Email = c.Email,
        HomePage = c.HomePage,
        Text = c.Text,
        CreatedAt = c.CreatedAt,
        Attachment = c.Attachment is null
            ? null
            : new AttachmentDto(
                c.Attachment.Id, c.Attachment.Type,
                c.Attachment.FileName, c.Attachment.ContentType, c.Attachment.SizeBytes)
    };

    private static PagedResult<CommentDto> Empty(int page) => new()
    {
        Items = [],
        Page = page,
        PageSize = PaginationDefaults.PageSize,
        TotalCount = 0
    };
}
