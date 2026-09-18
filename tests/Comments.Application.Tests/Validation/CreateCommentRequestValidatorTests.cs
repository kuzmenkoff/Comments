using Comments.Application.Dtos;
using Comments.Application.Validation;
using FluentValidation.TestHelper;
using Xunit;

namespace Comments.Application.Tests.Validation;

public class CreateCommentRequestValidatorTests
{
    private readonly CreateCommentRequestValidator _validator = new();

    private static CreateCommentRequest Valid() => new()
    {
        UserName = "John123",
        Email = "john@example.com",
        Text = "Hello",
        CaptchaId = "abc",
        CaptchaAnswer = "1234"
    };

    [Fact]
    public void Passes_for_a_valid_request()
    {
        _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("John Doe")]   // space
    [InlineData("Иван")]       // non-latin
    [InlineData("bad!")]       // symbol
    public void Rejects_invalid_username(string userName)
    {
        var req = Valid();
        req.UserName = userName;

        _validator.TestValidate(req)
            .ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Rejects_invalid_email()
    {
        var req = Valid();
        req.Email = "invalid-email";

        _validator.TestValidate(req)
            .ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Allows_empty_homepage_but_rejects_bad_url()
    {
        var req = Valid();

        req.HomePage = "";
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.HomePage);

        req.HomePage = "dgfdgdfg1234";
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.HomePage);
    }
}
