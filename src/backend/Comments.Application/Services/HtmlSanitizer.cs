using System.Xml;
using System.Xml.Linq;
using Comments.Application.Abstractions;
using Comments.Application.Common;

namespace Comments.Application.Services;

/// <summary>
/// Allows only &lt;a href title&gt;, &lt;code&gt;, &lt;i&gt;, &lt;strong&gt;.
/// Rejects anything else, malformed (unclosed) markup, and unsafe links.
/// </summary>
public class HtmlSanitizer : IHtmlSanitizer
{
    private static readonly HashSet<string> AllowedTags =
        new(StringComparer.OrdinalIgnoreCase) { "a", "code", "i", "strong" };

    private static readonly HashSet<string> AllowedAnchorAttributes =
        new(StringComparer.OrdinalIgnoreCase) { "href", "title" };

    public SanitizationResult Sanitize(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return SanitizationResult.Valid(string.Empty);

        XElement root;
        try
        {
            // Wrapping in a root lets the text hold several tags/plain text.
            // XML parsing enforces well-formed XHTML: unclosed tags throw here.
            root = XElement.Parse($"<root>{html}</root>", LoadOptions.PreserveWhitespace);
        }
        catch (XmlException ex)
        {
            return SanitizationResult.Invalid(
                $"Text is not valid XHTML - check that every tag is properly closed. ({ex.Message})");
        }

        var errors = new List<string>();
        Validate(root, errors);

        if (errors.Count > 0)
            return SanitizationResult.Invalid(errors);

        // Re-serialize the children (without the <root> wrapper).
        var cleaned = string.Concat(root.Nodes().Select(n => n.ToString(SaveOptions.DisableFormatting)));
        return SanitizationResult.Valid(cleaned);
    }

    /// <summary>Walks the tree; records an error for anything outside the allowlist.</summary>
    private static void Validate(XElement element, List<string> errors)
    {
        foreach (var el in element.Elements())
        {
            var name = el.Name.LocalName;

            if (!AllowedTags.Contains(name))
            {
                errors.Add($"Tag <{name}> is not allowed.");
                continue;
            }

            if (name.Equals("a", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var attr in el.Attributes())
                {
                    var attrName = attr.Name.LocalName;
                    if (!AllowedAnchorAttributes.Contains(attrName))
                        errors.Add($"Attribute '{attrName}' is not allowed on <a>.");
                    else if (attrName.Equals("href", StringComparison.OrdinalIgnoreCase)
                             && !IsSafeHref(attr.Value))
                        errors.Add("Only absolute http(s) links are allowed.");
                }
            }
            else if (el.HasAttributes)
            {
                errors.Add($"Tag <{name}> must not have attributes.");
            }

            Validate(el, errors); // recurse into nested content
        }
    }

    private static bool IsSafeHref(string href) =>
        Uri.TryCreate(href, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
