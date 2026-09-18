using Comments.Application.Common;

namespace Comments.Application.Abstractions;

/// <summary>Validates and cleans user-supplied HTML to a safe, well-formed subset.</summary>
public interface IHtmlSanitizer
{
    SanitizationResult Sanitize(string html);
}
