using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IdeasGroup.Kanban.WebApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("El token no contiene el identificador del usuario.");

        return Guid.Parse(value);
    }
}
