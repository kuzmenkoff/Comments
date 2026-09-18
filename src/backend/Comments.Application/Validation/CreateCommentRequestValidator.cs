using Comments.Application.Dtos;
using FluentValidation;

namespace Comments.Application.Validation;

/// <summary>Server-side validation rules for creating a comment.</summary>
public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .MaximumLength(64)
            .Matches("^[a-zA-Z0-9]+$")
            .WithMessage("User Name may contain only Latin letters and digits.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);

        RuleFor(x => x.HomePage)
            .Must(BeValidHttpUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.HomePage))
            .WithMessage("Home Page must be a valid URL.");

        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(10_000);

        RuleFor(x => x.CaptchaId).NotEmpty();
        RuleFor(x => x.CaptchaAnswer).NotEmpty();
    }

    private static bool BeValidHttpUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
