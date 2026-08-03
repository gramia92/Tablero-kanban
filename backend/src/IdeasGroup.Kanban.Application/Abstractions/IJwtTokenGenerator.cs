using IdeasGroup.Kanban.Domain.Entities;

namespace IdeasGroup.Kanban.Application.Abstractions;

public record GeneratedToken(string AccessToken, DateTime ExpiresAtUtc);

public interface IJwtTokenGenerator
{
    GeneratedToken Generate(User user);
}
