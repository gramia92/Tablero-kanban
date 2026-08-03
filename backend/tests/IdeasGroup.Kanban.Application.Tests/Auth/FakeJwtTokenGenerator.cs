using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Domain.Entities;

namespace IdeasGroup.Kanban.Application.Tests.Auth;

public class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    public GeneratedToken Generate(User user) => new($"fake-token-for-{user.Email}", DateTime.UtcNow.AddHours(1));
}
