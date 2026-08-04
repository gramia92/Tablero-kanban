using IdeasGroup.Kanban.Application.Reports;
using IdeasGroup.Kanban.Domain.Exceptions;
using IdeasGroup.Kanban.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeasGroup.Kanban.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/report")]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;

    public ReportsController(ReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet]
    public async Task<IActionResult> Export(Guid projectId, [FromQuery] ReportFormat format, CancellationToken cancellationToken)
    {
        try
        {
            var file = await _reportService.ExportProjectReportAsync(User.GetUserId(), projectId, format, cancellationToken);
            return File(file.Content, file.ContentType, file.FileName);
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
