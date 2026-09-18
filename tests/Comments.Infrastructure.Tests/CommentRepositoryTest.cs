using Comments.Application.Common;
using Comments.Domain.Entities;
using Comments.Domain.Enums;
using Comments.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Comments.Infrastructure.Tests;

public class CommentRepositoryTests(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    private static Comment NewComment(string name, long? parentId = null, int secondsOffset = 0) => new()
    {
        ParentId = parentId,
        UserName = name,
        Email = $"{name}@example.com",
        Text = "text",
        CreatedAt = DateTimeOffset.UtcNow.AddSeconds(secondsOffset)
    };

    [Fact]
    public async Task GetDescendantsAsync_returns_the_whole_subtree()
    {
        await fixture.ResetAsync();
        await using var ctx = fixture.CreateContext();
        var repo = new CommentRepository(ctx);

        // root -> a -> b (grandchild), root -> c
        var root = await repo.AddAsync(NewComment("Root"));
        var a = await repo.AddAsync(NewComment("A", root.Id));
        var b = await repo.AddAsync(NewComment("B", a.Id));
        var c = await repo.AddAsync(NewComment("C", root.Id));

        var descendants = await repo.GetDescendantsAsync([root.Id]);

        // The CTE must reach the grandchild (b), proving real recursion.
        descendants.Select(x => x.Id).Should().BeEquivalentTo([a.Id, b.Id, c.Id]);
    }

    [Fact]
    public async Task GetTopLevelAsync_paginates_and_defaults_to_LIFO()
    {
        await fixture.ResetAsync();
        await using var ctx = fixture.CreateContext();
        var repo = new CommentRepository(ctx);

        for (var i = 0; i < 30; i++)
            await repo.AddAsync(NewComment($"User{i:D2}", secondsOffset: i));

        var page1 = await repo.GetTopLevelAsync(1, 25, CommentSortField.CreatedAt, SortDirection.Descending);
        page1.Items.Should().HaveCount(25);
        page1.TotalCount.Should().Be(30);
        page1.Items[0].UserName.Should().Be("User29"); // newest first (LIFO)

        var page2 = await repo.GetTopLevelAsync(2, 25, CommentSortField.CreatedAt, SortDirection.Descending);
        page2.Items.Should().HaveCount(5);             // remaining 5
    }

    [Fact]
    public async Task GetTopLevelAsync_sorts_by_username_ascending()
    {
        await fixture.ResetAsync();
        await using var ctx = fixture.CreateContext();
        var repo = new CommentRepository(ctx);

        await repo.AddAsync(NewComment("Charlie"));
        await repo.AddAsync(NewComment("Alice"));
        await repo.AddAsync(NewComment("Bob"));

        var result = await repo.GetTopLevelAsync(1, 25, CommentSortField.UserName, SortDirection.Ascending);

        result.Items.Select(c => c.UserName).Should().ContainInOrder("Alice", "Bob", "Charlie");
    }

    [Fact]
    public async Task AddAsync_persists_comment_with_attachment()
    {
        await fixture.ResetAsync();
        await using var ctx = fixture.CreateContext();
        var repo = new CommentRepository(ctx);

        var comment = NewComment("WithFile");
        comment.Attachment = new Attachment
        {
            Type = AttachmentType.Text,
            FileName = "note.txt",
            ContentType = "text/plain",
            SizeBytes = 3,
            Content = [1, 2, 3]
        };
        await repo.AddAsync(comment);

        await using var verify = fixture.CreateContext();
        var loaded = await verify.Comments.Include(c => c.Attachment).FirstAsync(c => c.Id == comment.Id);
        loaded.Attachment.Should().NotBeNull();
        loaded.Attachment!.FileName.Should().Be("note.txt");
    }
}
