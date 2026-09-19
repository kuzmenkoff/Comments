using Comments.Application.Common;

namespace Comments.Application.Abstractions;

/// <summary>Generates image CAPTCHAs and verifies user answers.</summary>
public interface ICaptchaService
{
    CaptchaChallenge Generate();
    bool Verify(string captchaId, string answer);
}
