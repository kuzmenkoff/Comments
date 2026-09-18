using Comments.Application.Services;
using FluentAssertions;
using Xunit;

namespace Comments.Application.Tests.Services;

public class HtmlSanitizerTests
{
    private readonly HtmlSanitizer _sanitizer = new();

    [Fact]
    public void Allows_permitted_tags()
    {
        var r = _sanitizer.Sanitize("<strong>b</strong> <i>i</i> <code>x=1</code>");
        r.IsValid.Should().BeTrue();
        r.Value.Should().Contain("<strong>").And.Contain("<i>").And.Contain("<code>");
    }

    [Fact]
    public void Allows_anchor_with_href_and_title()
    {
        var r = _sanitizer.Sanitize("<a href=\"https://example.com\" title=\"t\">link</a>");
        r.IsValid.Should().BeTrue();
        r.Value.Should().Contain("https://example.com");
    }

    [Fact]
    public void Rejects_disallowed_tag()
    {
        _sanitizer.Sanitize("<script>alert(1)</script>").IsValid.Should().BeFalse();
    }

    [Fact]
    public void Rejects_unclosed_tag()
    {
        _sanitizer.Sanitize("<strong>oops").IsValid.Should().BeFalse();
    }

    [Fact]
    public void Rejects_javascript_href()
    {
        _sanitizer.Sanitize("<a href=\"javascript:alert(1)\">x</a>").IsValid.Should().BeFalse();
    }

    [Fact]
    public void Rejects_attributes_on_non_anchor_tags()
    {
        _sanitizer.Sanitize("<strong onclick=\"evil()\">t</strong>").IsValid.Should().BeFalse();
    }

    [Fact]
    public void Keeps_plain_text_unchanged()
    {
        var r = _sanitizer.Sanitize("just text");
        r.IsValid.Should().BeTrue();
        r.Value.Should().Be("just text");
    }
}
