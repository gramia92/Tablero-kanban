using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdeasGroup.Kanban.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly KanbanDbContext _dbContext;

    public UserRepository(KanbanDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _dbContext.Users.SingleOrDefaultAsync(u => u.Email == email.Trim().ToLower(), cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        _dbContext.Users.AnyAsync(u => u.Email == email.Trim().ToLower(), cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) =>
        await _dbContext.Users.Where(u => ids.Contains(u.Id)).ToListAsync(cancellationToken);
}
