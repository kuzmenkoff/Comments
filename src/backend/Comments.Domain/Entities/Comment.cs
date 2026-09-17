using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace Comments.Domain.Entities;

/// <summary>
/// A user comment. Top-level when <see cref="ParentId"/> is null, otherwise a reply,
/// forming a tree of unlimited depth.
/// </summary>
public class Comment
{
    public long Id { get; set; }

    public long? ParentId { get; set; }
    public Comment? Parent { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();

    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? HomePage { get; set; }

    public string Text { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public Attachment? Attachment
    {
        get; set;
    }
}
