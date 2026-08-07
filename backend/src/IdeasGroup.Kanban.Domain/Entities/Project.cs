using IdeasGroup.Kanban.Domain.Common;
using IdeasGroup.Kanban.Domain.Enums;
using IdeasGroup.Kanban.Domain.Exceptions;

namespace IdeasGroup.Kanban.Domain.Entities;

public class Project : AuditableEntity
{
    private readonly List<ProjectMember> _members = new();
    private readonly List<Label> _labels = new();

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid OwnerId { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? ExpectedEndDate { get; private set; }
    public ProjectStatus Status { get; private set; }

    public IReadOnlyCollection<ProjectMember> Members => _members.AsReadOnly();
    public IReadOnlyCollection<Label> Labels => _labels.AsReadOnly();

    private Project()
    {
    }

    public static Project Create(
        string name, string? description, Guid ownerId,
        DateTime? startDate = null, DateTime? expectedEndDate = null, ProjectStatus status = ProjectStatus.Planned)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre del proyecto es obligatorio.");
        }

        if (ownerId == Guid.Empty)
        {
            throw new DomainException("El proyecto debe tener un propietario.");
        }

        ValidateDateRange(startDate, expectedEndDate);

        var project = new Project
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            OwnerId = ownerId,
            StartDate = startDate,
            ExpectedEndDate = expectedEndDate,
            Status = status
        };

        project._members.Add(ProjectMember.Create(project.Id, ownerId, ProjectRole.Owner));

        return project;
    }

    public void Update(
        string name, string? description,
        DateTime? startDate, DateTime? expectedEndDate, ProjectStatus status)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre del proyecto es obligatorio.");
        }

        ValidateDateRange(startDate, expectedEndDate);

        Name = name.Trim();
        Description = description?.Trim();
        StartDate = startDate;
        ExpectedEndDate = expectedEndDate;
        Status = status;
        Touch();
    }

    private static void ValidateDateRange(DateTime? startDate, DateTime? expectedEndDate)
    {
        if (startDate.HasValue && expectedEndDate.HasValue && expectedEndDate.Value < startDate.Value)
        {
            throw new DomainException("La fecha de fin prevista no puede ser anterior a la fecha de inicio.");
        }
    }

    public ProjectMember AddMember(Guid userId, ProjectRole role)
    {
        if (_members.Any(m => m.UserId == userId))
        {
            throw new DomainException("El usuario ya pertenece al proyecto.");
        }

        var member = ProjectMember.Create(Id, userId, role);
        _members.Add(member);
        Touch();
        return member;
    }

    public void RemoveMember(Guid userId)
    {
        if (userId == OwnerId)
        {
            throw new DomainException("No se puede quitar al propietario del proyecto.");
        }

        var member = _members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new ProjectMemberNotFoundException();

        _members.Remove(member);
        Touch();
    }

    public Label AddLabel(string name, string color)
    {
        var label = Label.Create(Id, name, color);
        _labels.Add(label);
        Touch();
        return label;
    }
}
