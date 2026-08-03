using IdeasGroup.Kanban.Domain.Common;
using IdeasGroup.Kanban.Domain.Exceptions;

namespace IdeasGroup.Kanban.Domain.Entities;

public class Label : AuditableEntity
{
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Color { get; private set; } = null!;

    private Label()
    {
    }

    public static Label Create(Guid projectId, string name, string color)
    {
        if (projectId == Guid.Empty)
        {
            throw new DomainException("La etiqueta debe pertenecer a un proyecto.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre de la etiqueta es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(color))
        {
            throw new DomainException("El color de la etiqueta es obligatorio.");
        }

        return new Label
        {
            ProjectId = projectId,
            Name = name.Trim(),
            Color = color.Trim()
        };
    }

    public void Update(string name, string color)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre de la etiqueta es obligatorio.");
        }

        Name = name.Trim();
        Color = color.Trim();
        Touch();
    }
}
