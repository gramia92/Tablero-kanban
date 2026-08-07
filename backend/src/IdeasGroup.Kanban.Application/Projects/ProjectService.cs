using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Application.Common;
using IdeasGroup.Kanban.Domain.Entities;
using IdeasGroup.Kanban.Domain.Enums;
using IdeasGroup.Kanban.Domain.Exceptions;

namespace IdeasGroup.Kanban.Application.Projects;

public class ProjectService
{
    private static readonly string[] DefaultColumnNames = { "Por hacer", "En progreso", "Hecho" };

    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;

    public ProjectService(IProjectRepository projectRepository, IUserRepository userRepository)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
    }

    public async Task<ProjectResponse> CreateAsync(Guid ownerId, CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var project = Project.Create(
            request.Name, request.Description, ownerId,
            request.StartDate, request.ExpectedEndDate, request.Status);

        var board = Board.Create(project.Id, "Tablero principal");
        foreach (var columnName in DefaultColumnNames)
        {
            board.AddColumn(columnName);
        }

        await _projectRepository.AddAsync(project, board, cancellationToken);

        return await ToResponseAsync(project, cancellationToken);
    }

    public async Task<PagedResult<ProjectResponse>> ListForUserAsync(
        Guid userId, int page = 1, int pageSize = 10, string? search = null, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

        var (projects, totalCount) = await _projectRepository.ListForUserAsync(userId, page, pageSize, search, cancellationToken);
        var usersById = await GetUsersByIdAsync(projects.SelectMany(p => p.Members.Select(m => m.UserId)), cancellationToken);

        var items = projects.Select(p => BuildResponse(p, usersById)).ToList();
        return new PagedResult<ProjectResponse>(items, totalCount, page, pageSize);
    }

    public async Task<ProjectResponse> GetByIdAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await GetProjectOrThrowAsync(projectId, cancellationToken);
        EnsureMember(project, userId);

        return await ToResponseAsync(project, cancellationToken);
    }

    public async Task<ProjectResponse> UpdateAsync(Guid userId, Guid projectId, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var project = await GetProjectOrThrowAsync(projectId, cancellationToken);
        EnsureOwner(project, userId);

        project.Update(request.Name, request.Description, request.StartDate, request.ExpectedEndDate, request.Status);
        await _projectRepository.UpdateAsync(project, cancellationToken);

        return await ToResponseAsync(project, cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await GetProjectOrThrowAsync(projectId, cancellationToken);
        EnsureOwner(project, userId);

        await _projectRepository.DeleteAsync(project, cancellationToken);
    }

    public async Task<ProjectMemberResponse> AddMemberAsync(Guid userId, Guid projectId, AddMemberRequest request, CancellationToken cancellationToken = default)
    {
        var project = await GetProjectOrThrowAsync(projectId, cancellationToken);
        EnsureOwner(project, userId);

        var member = await _userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new DomainException("No existe ningún usuario registrado con ese email.");

        var newMember = project.AddMember(member.Id, ProjectRole.Member);
        await _projectRepository.AddMemberAsync(newMember, cancellationToken);

        return new ProjectMemberResponse(member.Id, member.FullName, member.Email, ProjectRole.Member.ToString());
    }

    public async Task RemoveMemberAsync(Guid userId, Guid projectId, Guid memberUserId, CancellationToken cancellationToken = default)
    {
        var project = await GetProjectOrThrowAsync(projectId, cancellationToken);
        EnsureOwner(project, userId);

        project.RemoveMember(memberUserId);
        await _projectRepository.UpdateAsync(project, cancellationToken);
    }

    private async Task<Project> GetProjectOrThrowAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return await _projectRepository.GetByIdAsync(projectId, cancellationToken)
            ?? throw new ProjectNotFoundException();
    }

    private static void EnsureMember(Project project, Guid userId)
    {
        if (project.Members.All(m => m.UserId != userId))
        {
            throw new ForbiddenProjectAccessException();
        }
    }

    private static void EnsureOwner(Project project, Guid userId)
    {
        if (project.OwnerId != userId)
        {
            throw new ForbiddenProjectAccessException();
        }
    }

    private async Task<ProjectResponse> ToResponseAsync(Project project, CancellationToken cancellationToken)
    {
        var usersById = await GetUsersByIdAsync(project.Members.Select(m => m.UserId), cancellationToken);
        return BuildResponse(project, usersById);
    }

    private async Task<Dictionary<Guid, User>> GetUsersByIdAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetByIdsAsync(userIds.Distinct(), cancellationToken);
        return users.ToDictionary(u => u.Id);
    }

    private static ProjectResponse BuildResponse(Project project, IReadOnlyDictionary<Guid, User> usersById)
    {
        var members = project.Members
            .Select(m =>
            {
                usersById.TryGetValue(m.UserId, out var user);
                return new ProjectMemberResponse(m.UserId, user?.FullName ?? string.Empty, user?.Email ?? string.Empty, m.Role.ToString());
            })
            .ToList();

        return new ProjectResponse(
            project.Id, project.Name, project.Description, project.OwnerId,
            project.StartDate, project.ExpectedEndDate, project.Status,
            project.CreatedAt, members);
    }
}
