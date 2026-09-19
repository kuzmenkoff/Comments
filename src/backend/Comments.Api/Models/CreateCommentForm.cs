using Comments.Application.Dtos;

namespace Comments.Api.Models;

public class CreateCommentForm
{
    public long? ParentId { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? HomePage { get; set; }
    public string Text { get; set; } = null!;
    public string CaptchaId { get; set; } = null!;
    public string CaptchaAnswer { get; set; } = null!;
    public IFormFile? File { get; set; }

    public async Task<CreateCommentRequest> ToRequestAsync(CancellationToken ct)
    {
        UploadedFile? uploaded = null;
        if (File is { Length: > 0 })
        {
            using var ms = new MemoryStream();
            await File.CopyToAsync(ms, ct);
            uploaded = new UploadedFile(File.FileName, File.ContentType, ms.ToArray());
        }

        return new CreateCommentRequest
        {
            ParentId = ParentId,
            UserName = UserName,
            Email = Email,
            HomePage = HomePage,
            Text = Text,
            CaptchaId = CaptchaId,
            CaptchaAnswer = CaptchaAnswer,
            File = uploaded
        };
    }
}
