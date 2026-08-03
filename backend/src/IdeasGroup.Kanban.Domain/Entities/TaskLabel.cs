using IdeasGroup.Kanban.Domain.Common;

namespace IdeasGroup.Kanban.Domain.Entities;

public class TaskLabel : Entity
{
    public Guid KanbanTaskId { get; private set; }
    public Guid LabelId { get; private set; }

    private TaskLabel()
    {
    }

    public static TaskLabel Create(Guid kanbanTaskId, Guid labelId)
    {
        return new TaskLabel
        {
            KanbanTaskId = kanbanTaskId,
            LabelId = labelId
        };
    }
}
