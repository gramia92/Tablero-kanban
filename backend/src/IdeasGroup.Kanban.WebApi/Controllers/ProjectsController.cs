using IdeasGroup.Kanban.Application.Projects;
using IdeasGroup.Kanban.Domain.Exceptions;
using IdeasGroup.Kanban.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeasGroup.Kanban.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _projectService;

    public ProjectsController(ProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectResponse>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _projectService.ListForUserAsync(User.GetUserId(), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> Create(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _projectService.CreateAsync(User.GetUserId(), request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _projectService.GetByIdAsync(User.GetUserId(), id, cancellationToken));
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

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectResponse>> Update(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _projectService.UpdateAsync(User.GetUserId(), id, request, cancellationToken));
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

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _projectService.DeleteAsync(User.GetUserId(), id, cancellationToken);
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
    }

    [HttpPost("{id:guid}/members")]
    public async Task<ActionResult<ProjectMemberResponse>> AddMember(Guid id, AddMemberRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _projectService.AddMemberAsync(User.GetUserId(), id, request, cancellationToken));
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

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            await _projectService.RemoveMemberAsync(User.GetUserId(), id, userId, cancellationToken);
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
        catch (ProjectMemberNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
