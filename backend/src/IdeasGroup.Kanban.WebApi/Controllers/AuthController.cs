using IdeasGroup.Kanban.Application.Auth;
using IdeasGroup.Kanban.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeasGroup.Kanban.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _authService.LoginAsync(request, cancellationToken));
        }
        catch (InvalidCredentialsException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.RegisterAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Login), response);
        }
        catch (EmailAlreadyRegisteredException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var claims = User.Claims.ToDictionary(c => c.Type, c => c.Value);
        return Ok(claims);
    }
}
