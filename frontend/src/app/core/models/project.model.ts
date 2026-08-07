export enum ProjectStatus {
    Planned = 'Planned',
    InProgress = 'InProgress',
    Completed = 'Completed',
    Cancelled = 'Cancelled'
}

export interface ProjectMember {
    userId: string;
    fullName: string;
    email: string;
    role: string;
}

export interface Project {
    id: string;
    name: string;
    description: string | null;
    ownerId: string;
    startDate: string | null;
    expectedEndDate: string | null;
    status: ProjectStatus;
    createdAtUtc: string;
    members: ProjectMember[];
}

export interface CreateProjectRequest {
    name: string;
    description?: string | null;
    startDate?: string | null;
    expectedEndDate?: string | null;
    status?: ProjectStatus;
}

export interface UpdateProjectRequest {
    name: string;
    description?: string | null;
    startDate: string | null;
    expectedEndDate: string | null;
    status: ProjectStatus;
}

export interface AddMemberRequest {
    email: string;
}

export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
}
