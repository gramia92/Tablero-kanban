namespace IdeasGroup.Kanban.Application.Tasks;

/// <summary>
/// El cliente indica el destino como "entre estas dos tarjetas visibles" (la posición
/// numérica interna es un detalle de persistencia, no un contrato de API).
/// </summary>
public record MoveTaskRequest(Guid TargetColumnId, Guid? PreviousTaskId, Guid? NextTaskId);
