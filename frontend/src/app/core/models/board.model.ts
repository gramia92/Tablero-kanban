export interface BoardColumn {
    id: string;
    name: string;
    order: number;
    taskCount: number;
}

export interface Board {
    id: string;
    projectId: string;
    name: string;
    columns: BoardColumn[];
}

export interface CreateColumnRequest {
    name: string;
}

export interface RenameColumnRequest {
    name: string;
}

export interface ReorderColumnsRequest {
    orderedColumnIds: string[];
}
