namespace Comments.Application.Common;

/// <summary>Outcome of sanitizing user HTML: either a clean value, or the reasons it was rejected.</summary>
public class SanitizationResult
{
    public bool IsValid { get; }
    public string Value { get; }
    public IReadOnlyList<string> Errors { get; }

    private SanitizationResult(bool isValid, string value, IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Value = value;
        Errors = errors;
    }

    public static SanitizationResult Valid(string value) => new(true, value, []);
    public static SanitizationResult Invalid(params string[] errors) => new(false, string.Empty, errors);
    public static SanitizationResult Invalid(IEnumerable<string> errors) => new(false, string.Empty, errors.ToList());
}
