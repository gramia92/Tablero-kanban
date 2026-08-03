using IdeasGroup.Kanban.Application.Abstractions;

namespace IdeasGroup.Kanban.Application.Tests.Auth;

public class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string plainPassword) => $"hashed:{plainPassword}";

    public bool Verify(string plainPassword, string passwordHash) => passwordHash == $"hashed:{plainPassword}";
}
