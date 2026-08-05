export enum Priority {
    Low = 'Low',
    Medium = 'Medium',
    High = 'High',
    Critical = 'Critical'
}

export interface KanbanTask {
    id: string;
    boardColumnId: string;
    title: string;
    description: string | null;
    position: number;
    priority: Priority;
    assigneeId: string | null;
    assigneeName: string | null;
    createdById: string;
    dueDate: string | null;
    createdAtUtc: string;
}

export interface CreateTaskRequest {
    boardColumnId: string;
    title: string;
    description?: string | null;
    priority: Priority;
    assigneeId?: string | null;
    dueDate?: string | null;
}

export interface UpdateTaskRequest {
    title: string;
    description?: string | null;
    priority: Priority;
    assigneeId?: string | null;
    dueDate?: string | null;
}

export interface MoveTaskRequest {
    targetColumnId: string;
    previousTaskId?: string | null;
    nextTaskId?: string | null;
}
