import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { CreateTaskRequest, KanbanTask, MoveTaskRequest, UpdateTaskRequest } from '../models/task.model';

@Injectable({ providedIn: 'root' })
export class TaskService {

    constructor(private http: HttpClient) { }

    private baseUrl(projectId: string): string {
        return `${environment.apiUrl}/projects/${projectId}/tasks`;
    }

    list(projectId: string): Observable<KanbanTask[]> {
        return this.http.get<KanbanTask[]>(this.baseUrl(projectId));
    }

    create(projectId: string, request: CreateTaskRequest): Observable<KanbanTask> {
        return this.http.post<KanbanTask>(this.baseUrl(projectId), request);
    }

    update(projectId: string, taskId: string, request: UpdateTaskRequest): Observable<KanbanTask> {
        return this.http.put<KanbanTask>(`${this.baseUrl(projectId)}/${taskId}`, request);
    }

    delete(projectId: string, taskId: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl(projectId)}/${taskId}`);
    }

    move(projectId: string, taskId: string, request: MoveTaskRequest): Observable<KanbanTask> {
        return this.http.put<KanbanTask>(`${this.baseUrl(projectId)}/${taskId}/move`, request);
    }
}
