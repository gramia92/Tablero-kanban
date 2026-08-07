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
    createdAtUtc: string;
    members: ProjectMember[];
}

export interface CreateProjectRequest {
    name: string;
    description?: string | null;
}

export interface UpdateProjectRequest {
    name: string;
    description?: string | null;
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
