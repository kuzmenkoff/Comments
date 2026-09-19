using Comments.Infrastructure.Services;
using FluentAssertions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace Comments.Infrastructure.Tests;

public class ImageSharpProcessorTests
{
    [Fact]
    public void ResizeToFit_shrinks_large_image_within_bounds()
    {
        using var img = new Image<Rgba32>(640, 480);
        using var ms = new MemoryStream();
        img.SaveAsPng(ms);

        var result = new ImageSharpProcessor().ResizeToFit(ms.ToArray(), 320, 240);

        result.Width.Should().BeLessThanOrEqualTo(320);
        result.Height.Should().BeLessThanOrEqualTo(240);
        result.ContentType.Should().Be("image/png");
    }

    [Fact]
    public void ResizeToFit_rejects_non_image()
    {
        var act = () => new ImageSharpProcessor().ResizeToFit([1, 2, 3], 320, 240);
        act.Should().Throw<Exception>();
    }
}
