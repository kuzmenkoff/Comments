using Comments.Application.Abstractions;
using Comments.Application.Common;
using Comments.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Comments.Api.Controllers;

/// <summary>HTTP endpoints for reading and creating comments.</summary>
[ApiController]
[Route("api/[controller]")]
public class CommentsController(ICommentService commentService) : ControllerBase
{
    /// <summary>
    /// Returns a page of top-level comments with their reply trees.
    /// Sortable by user name, e-mail or date; default order is LIFO (newest first).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<CommentDto>>> GetPage(
        [FromQuery] int page = 1,
        [FromQuery] CommentSortField sort = CommentSortField.CreatedAt,
        [FromQuery] SortDirection direction = SortDirection.Descending,
        CancellationToken ct = default)
    {
        var result = await commentService.GetPageAsync(page, sort, direction, ct);
        return Ok(result);
    }

    /// <summary>Creates a comment, or a reply when ParentId is set.</summary>
    [HttpPost]
    public async Task<ActionResult<CommentDto>> Create(
        [FromBody] CreateCommentRequest request, CancellationToken ct)
    {
        var created = await commentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetPage), created);
    }
}
