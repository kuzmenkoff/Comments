
using Comments.Application.Abstractions;
using Comments.Application.Common;
using Comments.Application.Dtos;
using Comments.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;

namespace Comments.Application.Services;

/// <summary>Reads comments and assembles flat rows into reply trees.</summary>
public class CommentService(
    ICommentRepository repository,
    IValidator<CreateCommentRequest> validator,
    IHtmlSanitizer sanitizer,
    ICaptchaService captcha,
    IAttachmentService attachments) : ICommentService
{
    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<CommentDto> CreateAsync(CreateCommentRequest request, CancellationToken ct = default)
    {
        // 1. Field validation (UserName/Email/HomePage/Text/Captcha* format).
        await validator.ValidateAndThrowAsync(request, ct);

        if (!captcha.Verify(request.CaptchaId, request.CaptchaAnswer))
            throw new ValidationException(
                [new ValidationFailure(nameof(request.CaptchaAnswer), "Incorrect CAPTCHA.")]);

        // 2. Sanitize the text.
        var sanitized = sanitizer.Sanitize(request.Text);
        if (!sanitized.IsValid)
            throw new ValidationException(
                sanitized.Errors.Select(e => new ValidationFailure(nameof(request.Text), e)));

        // 3. A reply must point to an existing parent.
        if (request.ParentId is { } parentId && !await repository.ExistsAsync(parentId, ct))
            throw new ValidationException(
                [new ValidationFailure(nameof(request.ParentId), "Parent comment does not exist.")]);

        // 4. Build and persist.
        var comment = new Comment
        {
            ParentId = request.ParentId,
            UserName = request.UserName,
            Email = request.Email,
            HomePage = request.HomePage,
            Text = sanitized.Value,
            CreatedAt = DateTimeOffset.UtcNow,
            Attachment = request.File is null ? null : attachments.Create(request.File)
        };

        var saved = await repository.AddAsync(comment, ct);
        return MapToDto(saved);
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

    /// <inheritdoc />
    public async Task<AttachmentContent?> GetAttachmentAsync(long attachmentId, CancellationToken ct = default)
    {
        var att = await repository.GetAttachmentAsync(attachmentId, ct);
        return att is null ? null : new AttachmentContent(att.Content, att.ContentType, att.FileName);
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
