using Comments.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Comments.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttachmentsController(ICommentService commentService) : ControllerBase
{
    /// <summary>Returns the raw attachment bytes (used by the lightbox / download link).</summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
    {
        var file = await commentService.GetAttachmentAsync(id, ct);
        return file is null ? NotFound() : File(file.Content, file.ContentType, file.FileName);
    }
}
