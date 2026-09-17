using Comments.Application.Abstractions;
using Comments.Application.Common;
using Comments.Domain.Entities;
using Comments.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Comments.Infrastructure.Repositories;

/// <summary>EF Core implementation of the comment data access.</summary>
public class CommentRepository(CommentsDbContext db) : ICommentRepository
{
    public async Task<PagedResult<Comment>> GetTopLevelAsync(
        int page, int pageSize,
        CommentSortField sortField, SortDirection direction,
        CancellationToken ct = default)
    {
        var query = db.Comments.AsNoTracking().Where(c => c.ParentId == null);

        // Dynamic ordering with a stable tie-breaker (Id) so paging is deterministic.
        IOrderedQueryable<Comment> ordered = (sortField, direction) switch
        {
            (CommentSortField.UserName, SortDirection.Ascending) => query.OrderBy(c => c.UserName),
            (CommentSortField.UserName, SortDirection.Descending) => query.OrderByDescending(c => c.UserName),
            (CommentSortField.Email, SortDirection.Ascending) => query.OrderBy(c => c.Email),
            (CommentSortField.Email, SortDirection.Descending) => query.OrderByDescending(c => c.Email),
            (_, SortDirection.Ascending) => query.OrderBy(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt) // LIFO
        };
        query = ordered.ThenByDescending(c => c.Id);

        var total = await query.CountAsync(ct);

        var items = await query
            .Include(c => c.Attachment)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Comment>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<IReadOnlyList<Comment>> GetDescendantsAsync(
        IReadOnlyCollection<long> rootIds, CancellationToken ct = default)
    {
        if (rootIds.Count == 0) return [];

        // Parameterized IN list (@p0, @p1, ...) — safe against SQL injection.
        var ids = rootIds.ToArray();
        var placeholders = string.Join(", ", ids.Select((_, i) => $"@p{i}"));
        var parameters = ids.Select((id, i) => new SqlParameter($"@p{i}", id)).ToArray();

        // Recursive CTE walks the whole reply subtree in one round-trip.
        // MAXRECURSION 0 removes SQL Server's default 100-level limit.
        var sql = $@"
WITH Descendants AS (
    SELECT * FROM Comments WHERE ParentId IN ({placeholders})
    UNION ALL
    SELECT c.* FROM Comments c
    INNER JOIN Descendants d ON c.ParentId = d.Id
)
SELECT * FROM Descendants
OPTION (MAXRECURSION 0)";

        // A CTE cannot be composed as a subquery, so we can't chain Include here.
        var comments = await db.Comments
            .FromSqlRaw(sql, parameters)
            .AsNoTracking()
            .ToListAsync(ct);

        // Load attachments for these comments in a second query and stitch them in.
        var commentIds = comments.Select(c => c.Id).ToArray();
        var attachments = await db.Attachments
            .AsNoTracking()
            .Where(a => commentIds.Contains(a.CommentId))
            .ToListAsync(ct);

        var byCommentId = attachments.ToDictionary(a => a.CommentId);
        foreach (var c in comments)
            if (byCommentId.TryGetValue(c.Id, out var a))
                c.Attachment = a;

        return comments;
    }

    public Task<bool> ExistsAsync(long id, CancellationToken ct = default) =>
        db.Comments.AnyAsync(c => c.Id == id, ct);

    public async Task<Comment> AddAsync(Comment comment, CancellationToken ct = default)
    {
        db.Comments.Add(comment);           // cascades the attachment too, if set
        await db.SaveChangesAsync(ct);
        return comment;
    }

    public Task<Attachment?> GetAttachmentAsync(long attachmentId, CancellationToken ct = default) =>
        db.Attachments.AsNoTracking().FirstOrDefaultAsync(a => a.Id == attachmentId, ct);
}