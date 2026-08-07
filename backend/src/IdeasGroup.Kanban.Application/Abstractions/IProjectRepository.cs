using IdeasGroup.Kanban.Domain.Entities;

namespace IdeasGroup.Kanban.Application.Abstractions;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Project> Items, int TotalCount)> ListForUserAsync(
        Guid userId, int page, int pageSize, string? search, CancellationToken cancellationToken = default);

    Task AddAsync(Project project, Board board, CancellationToken cancellationToken = default);

    Task UpdateAsync(Project project, CancellationToken cancellationToken = default);

    Task AddMemberAsync(ProjectMember member, CancellationToken cancellationToken = default);

    Task DeleteAsync(Project project, CancellationToken cancellationToken = default);
}
