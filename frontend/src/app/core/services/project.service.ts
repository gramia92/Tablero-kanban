import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { AddMemberRequest, CreateProjectRequest, Project, ProjectMember, UpdateProjectRequest } from '../models/project.model';

@Injectable({ providedIn: 'root' })
export class ProjectService {

    private readonly baseUrl = `${environment.apiUrl}/projects`;

    constructor(private http: HttpClient) { }

    list(): Observable<Project[]> {
        return this.http.get<Project[]>(this.baseUrl);
    }

    getById(id: string): Observable<Project> {
        return this.http.get<Project>(`${this.baseUrl}/${id}`);
    }

    create(request: CreateProjectRequest): Observable<Project> {
        return this.http.post<Project>(this.baseUrl, request);
    }

    update(id: string, request: UpdateProjectRequest): Observable<Project> {
        return this.http.put<Project>(`${this.baseUrl}/${id}`, request);
    }

    delete(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }

    addMember(id: string, request: AddMemberRequest): Observable<ProjectMember> {
        return this.http.post<ProjectMember>(`${this.baseUrl}/${id}/members`, request);
    }

    removeMember(id: string, userId: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}/members/${userId}`);
    }
}
