using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Domain.Entities;

namespace IdeasGroup.Kanban.Application.Tests.Auth;

public class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public FakeUserRepository Seed(User user)
    {
        _users.Add(user);
        return this;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.SingleOrDefault(u => u.Email == email.Trim().ToLowerInvariant()));

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.Any(u => u.Email == email.Trim().ToLowerInvariant()));

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<User>>(_users.Where(u => ids.Contains(u.Id)).ToList());
}
