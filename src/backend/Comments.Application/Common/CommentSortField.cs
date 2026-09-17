namespace Comments.Application.Common;

/// <summary>Fields the top-level comments table can be sorted by.</summary>
public enum CommentSortField
{
    CreatedAt = 0, // default (LIFO)
    UserName = 1,
    Email = 2
}