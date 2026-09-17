namespace Comments.Application.Dtos;

/// <summary>Incoming data to create a comment or a reply.</summary>
public class CreateCommentRequest
{
    public long? ParentId { get; set; }

    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? HomePage { get; set; }
    public string Text { get; set; } = null!;

    /// <summary>Id of the CAPTCHA challenge issued earlier (looked up in cache).</summary>
    public string CaptchaId { get; set; } = null!;

    /// <summary>User's CAPTCHA answer, verified against the stored one.</summary>
    public string CaptchaAnswer { get; set; } = null!;

    /// <summary>Optional attachment (image or txt); null when none.</summary>
    public UploadedFile? File { get; set; }
}
