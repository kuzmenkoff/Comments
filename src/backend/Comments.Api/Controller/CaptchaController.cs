using Comments.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Comments.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CaptchaController(ICaptchaService captchaService) : ControllerBase
{
    /// <summary>Issues a new CAPTCHA: an id plus the image as a data-URI for the SPA.</summary>
    [HttpGet]
    public ActionResult<CaptchaResponse> Get()
    {
        var challenge = captchaService.Generate();
        return Ok(new CaptchaResponse(challenge.Id, challenge.ImageDataUri));
    }
}

public record CaptchaResponse(string CaptchaId, string Image);
