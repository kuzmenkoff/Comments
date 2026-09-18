namespace Comments.Application.Common;

/// <summary>A generated CAPTCHA: an id to verify against, and the image as a data-URI.</summary>
public record CaptchaChallenge(string Id, string ImageDataUri);
