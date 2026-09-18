namespace Comments.Application.Dtos;

/// <summary>A comment returned to the client, with its nested replies for cascading display.</summary>
public class CommentDto
{
    public long Id { get; set; }
    public long? ParentId { get; set; }

    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? HomePage { get; set; }
    public string Text { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }

    public AttachmentDto? Attachment { get; set; }

    /// <summary>Nested replies (built by the service into a tree).</summary>
    public List<CommentDto> Replies { get; set; } = [];
}
