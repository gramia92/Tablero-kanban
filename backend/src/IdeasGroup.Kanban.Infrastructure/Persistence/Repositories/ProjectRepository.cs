using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdeasGroup.Kanban.Infrastructure.Persistence.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly KanbanDbContext _dbContext;

    public ProjectRepository(KanbanDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Project?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        _dbContext.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

    public async Task<IReadOnlyList<Project>> ListForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _dbContext.Projects
            .Include(p => p.Members)
            .Where(p => p.Members.Any(m => m.UserId == userId))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Project project, Board board, CancellationToken cancellationToken = default)
    {
        _dbContext.Projects.Add(project);
        _dbContext.Boards.Add(board);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(Project project, CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    // Los IDs se generan en el cliente (Entity.Id), así que al agregar un miembro nuevo a
    // la colección de un Project ya rastreado, EF no tiene forma de distinguirlo de una fila
    // existente (el valor de la key ya no es el "default" que usa para detectar altas) y
    // termina generando un UPDATE de 0 filas en vez de un INSERT. Por eso el alta se hace
    // explícita aquí en lugar de depender de UpdateAsync + detección automática de cambios.
    public async Task AddMemberAsync(ProjectMember member, CancellationToken cancellationToken = default)
    {
        _dbContext.ProjectMembers.Add(member);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Project project, CancellationToken cancellationToken = default)
    {
        _dbContext.Projects.Remove(project);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
