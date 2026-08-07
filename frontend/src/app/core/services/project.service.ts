import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { AddMemberRequest, CreateProjectRequest, PagedResult, Project, ProjectMember, UpdateProjectRequest } from '../models/project.model';

@Injectable({ providedIn: 'root' })
export class ProjectService {

    private readonly baseUrl = `${environment.apiUrl}/projects`;

    constructor(private http: HttpClient) { }

    list(page = 1, pageSize = 10, search: string | null = null): Observable<PagedResult<Project>> {
        let params = new HttpParams().set('page', page).set('pageSize', pageSize);
        if (search) {
            params = params.set('search', search);
        }
        return this.http.get<PagedResult<Project>>(this.baseUrl, { params });
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
