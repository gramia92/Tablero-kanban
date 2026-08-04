using IdeasGroup.Kanban.Application.Tasks;
using IdeasGroup.Kanban.Domain.Exceptions;
using IdeasGroup.Kanban.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeasGroup.Kanban.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/tasks")]
public class TasksController : ControllerBase
{
    private readonly TaskService _taskService;

    public TasksController(TaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> List(Guid projectId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _taskService.ListByProjectAsync(User.GetUserId(), projectId, cancellationToken));
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

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(Guid projectId, CreateTaskRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _taskService.CreateAsync(User.GetUserId(), projectId, request, cancellationToken);
            return CreatedAtAction(nameof(List), new { projectId }, response);
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

    [HttpPut("{taskId:guid}")]
    public async Task<ActionResult<TaskResponse>> Update(Guid projectId, Guid taskId, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _taskService.UpdateAsync(User.GetUserId(), projectId, taskId, request, cancellationToken));
        }
        catch (ProjectNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ForbiddenProjectAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (TaskNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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

    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid taskId, CancellationToken cancellationToken)
    {
        try
        {
            await _taskService.DeleteAsync(User.GetUserId(), projectId, taskId, cancellationToken);
            return NoContent();
        }
        catch (ProjectNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ForbiddenProjectAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (TaskNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BoardColumnNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{taskId:guid}/move")]
    public async Task<ActionResult<TaskResponse>> Move(Guid projectId, Guid taskId, MoveTaskRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _taskService.MoveAsync(User.GetUserId(), projectId, taskId, request, cancellationToken));
        }
        catch (ProjectNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ForbiddenProjectAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (TaskNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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
}
