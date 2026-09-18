using Comments.Application.Abstractions;
using Comments.Application.Common;
using Comments.Domain.Entities;
using Comments.Application.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Comments.Application.Tests.Services;

public class CommentServiceTests
{
    [Fact]
    public async Task GetPageAsync_assembles_nested_reply_tree()
    {
        // Arrange: one root (1) → reply (2) → reply (3)
        var root = new Comment { Id = 1, CreatedAt = DateTimeOffset.UtcNow };
        var page = new PagedResult<Comment>
        {
            Items = [root],
            Page = 1,
            PageSize = 25,
            TotalCount = 1
        };
        var descendants = new List<Comment>
        {
            new() { Id = 2, ParentId = 1, CreatedAt = DateTimeOffset.UtcNow.AddMinutes(1) },
            new() { Id = 3, ParentId = 2, CreatedAt = DateTimeOffset.UtcNow.AddMinutes(2) }
        };

        var repo = new Mock<ICommentRepository>();
        repo.Setup(r => r.GetTopLevelAsync(1, 25,
                It.IsAny<CommentSortField>(), It.IsAny<SortDirection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);
        repo.Setup(r => r.GetDescendantsAsync(
                It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(descendants);

        var service = new CommentService(repo.Object);

        var result = await service.GetPageAsync(1, CommentSortField.CreatedAt, SortDirection.Descending);

        // Assert: tree shape 1 → 2 → 3
        result.Items.Should().HaveCount(1);
        var r1 = result.Items[0];
        r1.Id.Should().Be(1);
        r1.Replies.Should().ContainSingle().Which.Id.Should().Be(2);
        r1.Replies[0].Replies.Should().ContainSingle().Which.Id.Should().Be(3);
    }
}
