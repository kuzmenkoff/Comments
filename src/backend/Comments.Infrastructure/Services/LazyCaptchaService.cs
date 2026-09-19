using Comments.Application.Abstractions;
using Comments.Application.Common;
using Lazy.Captcha.Core;

namespace Comments.Infrastructure.Services;

/// <summary>Adapts Lazy.Captcha.Core to our ICaptchaService abstraction.</summary>
public class LazyCaptchaService(ICaptcha captcha) : ICaptchaService
{
    public CaptchaChallenge Generate()
    {
        var id = Guid.NewGuid().ToString("N");
        var data = captcha.Generate(id);           // renders image + stores the code under this id
        var image = data.Base64.StartsWith("data:")
            ? data.Base64
            : $"data:image/png;base64,{data.Base64}";
        return new CaptchaChallenge(id, image);
    }

    public bool Verify(string captchaId, string answer)
    {
        if (string.IsNullOrWhiteSpace(captchaId) || string.IsNullOrWhiteSpace(answer))
            return false;
        return captcha.Validate(captchaId, answer);  // validates and consumes by default
    }
}
