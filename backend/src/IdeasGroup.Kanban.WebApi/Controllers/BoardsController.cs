using IdeasGroup.Kanban.Application.Boards;
using IdeasGroup.Kanban.Domain.Exceptions;
using IdeasGroup.Kanban.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeasGroup.Kanban.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/board")]
public class BoardsController : ControllerBase
{
    private readonly BoardService _boardService;

    public BoardsController(BoardService boardService)
    {
        _boardService = boardService;
    }

    [HttpGet]
    public async Task<ActionResult<BoardResponse>> Get(Guid projectId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _boardService.GetByProjectIdAsync(User.GetUserId(), projectId, cancellationToken));
        }
        catch (ProjectNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ForbiddenProjectAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpPost("columns")]
    public async Task<ActionResult<ColumnResponse>> AddColumn(Guid projectId, CreateColumnRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _boardService.AddColumnAsync(User.GetUserId(), projectId, request, cancellationToken));
        }
        catch (ProjectNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ForbiddenProjectAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("columns/{columnId:guid}")]
    public async Task<ActionResult<BoardResponse>> RenameColumn(Guid projectId, Guid columnId, RenameColumnRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _boardService.RenameColumnAsync(User.GetUserId(), projectId, columnId, request, cancellationToken));
        }
        catch (ProjectNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ForbiddenProjectAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (BoardColumnNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("columns/{columnId:guid}")]
    public async Task<ActionResult<BoardResponse>> DeleteColumn(Guid projectId, Guid columnId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _boardService.DeleteColumnAsync(User.GetUserId(), projectId, columnId, cancellationToken));
        }
        catch (ProjectNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ForbiddenProjectAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (BoardColumnNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ColumnHasTasksException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("columns/reorder")]
    public async Task<ActionResult<BoardResponse>> ReorderColumns(Guid projectId, ReorderColumnsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _boardService.ReorderColumnsAsync(User.GetUserId(), projectId, request, cancellationToken));
        }
        catch (ProjectNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ForbiddenProjectAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
