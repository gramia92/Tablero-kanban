using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Domain.Entities;

namespace IdeasGroup.Kanban.Application.Tests.Projects;

public class FakeProjectRepository : IProjectRepository
{
    private readonly List<Project> _projects = new();
    public readonly List<Board> Boards = new();

    public FakeProjectRepository Seed(Project project)
    {
        _projects.Add(project);
        return this;
    }

    public Task<Project?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_projects.SingleOrDefault(p => p.Id == projectId));

    public Task<(IReadOnlyList<Project> Items, int TotalCount)> ListForUserAsync(
        Guid userId, int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        var query = _projects.Where(p => p.Members.Any(m => m.UserId == userId));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(pattern));
        }

        var all = query.OrderByDescending(p => p.CreatedAt).ToList();
        var pagedItems = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult<(IReadOnlyList<Project>, int)>((pagedItems, all.Count));
    }

    public Task AddAsync(Project project, Board board, CancellationToken cancellationToken = default)
    {
        _projects.Add(project);
        Boards.Add(board);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Project project, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task AddMemberAsync(ProjectMember member, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task DeleteAsync(Project project, CancellationToken cancellationToken = default)
    {
        _projects.Remove(project);
        return Task.CompletedTask;
    }
}
