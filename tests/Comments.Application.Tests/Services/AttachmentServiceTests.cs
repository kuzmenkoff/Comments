using Comments.Application.Abstractions;
using Comments.Application.Common;
using Comments.Application.Dtos;
using Comments.Application.Services;
using Comments.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using Moq;
using Xunit;

namespace Comments.Application.Tests.Services;

public class AttachmentServiceTests
{
    private static UploadedFile File(string name, byte[]? content = null) =>
        new(name, "application/octet-stream", content ?? [1, 2, 3]);

    [Fact]
    public void Create_image_resizes_and_returns_image_attachment()
    {
        var processor = new Mock<IImageProcessor>();
        processor.Setup(p => p.ResizeToFit(It.IsAny<byte[]>(), 320, 240))
            .Returns(new ProcessedImage([9, 9], "image/png", 320, 200));

        var att = new AttachmentService(processor.Object).Create(File("photo.png"));

        att.Type.Should().Be(AttachmentType.Image);
        att.ContentType.Should().Be("image/png");
        processor.Verify(p => p.ResizeToFit(It.IsAny<byte[]>(), 320, 240), Times.Once);
    }

    [Fact]
    public void Create_text_within_limit_returns_text_attachment()
    {
        var att = new AttachmentService(Mock.Of<IImageProcessor>()).Create(File("note.txt", new byte[100]));
        att.Type.Should().Be(AttachmentType.Text);
        att.ContentType.Should().Be("text/plain");
    }

    [Fact]
    public void Create_text_too_large_throws()
    {
        var svc = new AttachmentService(Mock.Of<IImageProcessor>());
        var act = () => svc.Create(File("big.txt", new byte[AttachmentRules.MaxTextBytes + 1]));
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_unsupported_type_throws()
    {
        var svc = new AttachmentService(Mock.Of<IImageProcessor>());
        var act = () => svc.Create(File("evil.exe"));
        act.Should().Throw<ValidationException>();
    }
}
