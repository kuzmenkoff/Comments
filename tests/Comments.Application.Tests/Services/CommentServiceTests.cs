using Comments.Application.Abstractions;
using Comments.Application.Common;
using Comments.Application.Dtos;
using Comments.Application.Services;
using Comments.Application.Validation;
using Comments.Domain.Entities;
using Comments.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using Moq;
using Xunit;

namespace Comments.Application.Tests.Services;

public class CommentServiceTests
{
    // Builds a service with the real validator + sanitizer and a given (mocked) repository.
    private static CommentService CreateService(ICommentRepository repo) =>
        new(repo, new CreateCommentRequestValidator(), new HtmlSanitizer());

    private static CreateCommentRequest ValidCreate() => new()
    {
        UserName = "John123",
        Email = "john@example.com",
        Text = "Hello <strong>world</strong>",
        CaptchaId = "c",
        CaptchaAnswer = "1234"
    };

    // ---------- Read / tree assembly ----------

    [Fact]
    public async Task GetPageAsync_assembles_nested_reply_tree()
    {
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

        var result = await CreateService(repo.Object)
            .GetPageAsync(1, CommentSortField.CreatedAt, SortDirection.Descending);

        result.Items.Should().HaveCount(1);
        var r1 = result.Items[0];
        r1.Id.Should().Be(1);
        r1.Replies.Should().ContainSingle().Which.Id.Should().Be(2);
        r1.Replies[0].Replies.Should().ContainSingle().Which.Id.Should().Be(3);
    }

    [Fact]
    public async Task GetPageAsync_returns_empty_when_no_roots()
    {
        var repo = new Mock<ICommentRepository>();
        repo.Setup(r => r.GetTopLevelAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<CommentSortField>(), It.IsAny<SortDirection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Comment> { Items = [], Page = 1, PageSize = 25, TotalCount = 0 });

        var result = await CreateService(repo.Object)
            .GetPageAsync(1, CommentSortField.CreatedAt, SortDirection.Descending);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        repo.Verify(r => r.GetDescendantsAsync(
            It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetPageAsync_orders_replies_oldest_first()
    {
        var root = new Comment { Id = 1, CreatedAt = DateTimeOffset.UtcNow };
        var page = new PagedResult<Comment> { Items = [root], Page = 1, PageSize = 25, TotalCount = 1 };
        var descendants = new List<Comment>
        {
            new() { Id = 3, ParentId = 1, CreatedAt = DateTimeOffset.UtcNow.AddMinutes(2) }, // newer
            new() { Id = 2, ParentId = 1, CreatedAt = DateTimeOffset.UtcNow.AddMinutes(1) }  // older
        };

        var repo = new Mock<ICommentRepository>();
        repo.Setup(r => r.GetTopLevelAsync(1, 25, It.IsAny<CommentSortField>(),
                It.IsAny<SortDirection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);
        repo.Setup(r => r.GetDescendantsAsync(
                It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(descendants);

        var result = await CreateService(repo.Object)
            .GetPageAsync(1, CommentSortField.CreatedAt, SortDirection.Descending);

        result.Items[0].Replies.Select(r => r.Id).Should().Equal(2, 3);
    }

    [Fact]
    public async Task GetPageAsync_maps_attachment_metadata()
    {
        var root = new Comment
        {
            Id = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            Attachment = new Attachment
            {
                Id = 9,
                Type = AttachmentType.Image,
                FileName = "pic.png",
                ContentType = "image/png",
                SizeBytes = 1234,
                Content = [1, 2, 3]
            }
        };
        var page = new PagedResult<Comment> { Items = [root], Page = 1, PageSize = 25, TotalCount = 1 };

        var repo = new Mock<ICommentRepository>();
        repo.Setup(r => r.GetTopLevelAsync(1, 25, It.IsAny<CommentSortField>(),
                It.IsAny<SortDirection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);
        repo.Setup(r => r.GetDescendantsAsync(
                It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await CreateService(repo.Object)
            .GetPageAsync(1, CommentSortField.CreatedAt, SortDirection.Descending);

        var dto = result.Items[0].Attachment;
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(9);
        dto.FileName.Should().Be("pic.png");
        dto.Type.Should().Be(AttachmentType.Image);
    }

    // ---------- Create ----------

    [Fact]
    public async Task CreateAsync_saves_sanitized_comment()
    {
        var repo = new Mock<ICommentRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comment c, CancellationToken _) => { c.Id = 42; return c; });

        var dto = await CreateService(repo.Object).CreateAsync(ValidCreate());

        dto.Id.Should().Be(42);
        dto.Text.Should().Contain("<strong>");
        repo.Verify(r => r.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_rejects_xss_text()
    {
        var repo = new Mock<ICommentRepository>();
        var req = ValidCreate();
        req.Text = "<script>alert(1)</script>";

        var act = () => CreateService(repo.Object).CreateAsync(req);

        await act.Should().ThrowAsync<ValidationException>();
        repo.Verify(r => r.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_rejects_reply_to_missing_parent()
    {
        var repo = new Mock<ICommentRepository>();
        repo.Setup(r => r.ExistsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var req = ValidCreate();
        req.ParentId = 999;

        var act = () => CreateService(repo.Object).CreateAsync(req);

        await act.Should().ThrowAsync<ValidationException>();
    }
}