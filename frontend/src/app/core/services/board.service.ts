import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Board, BoardColumn, CreateColumnRequest, RenameColumnRequest, ReorderColumnsRequest } from '../models/board.model';

@Injectable({ providedIn: 'root' })
export class BoardService {

    constructor(private http: HttpClient) { }

    private baseUrl(projectId: string): string {
        return `${environment.apiUrl}/projects/${projectId}/board`;
    }

    get(projectId: string): Observable<Board> {
        return this.http.get<Board>(this.baseUrl(projectId));
    }

    addColumn(projectId: string, request: CreateColumnRequest): Observable<BoardColumn> {
        return this.http.post<BoardColumn>(`${this.baseUrl(projectId)}/columns`, request);
    }

    renameColumn(projectId: string, columnId: string, request: RenameColumnRequest): Observable<Board> {
        return this.http.put<Board>(`${this.baseUrl(projectId)}/columns/${columnId}`, request);
    }

    deleteColumn(projectId: string, columnId: string): Observable<Board> {
        return this.http.delete<Board>(`${this.baseUrl(projectId)}/columns/${columnId}`);
    }

    reorderColumns(projectId: string, request: ReorderColumnsRequest): Observable<Board> {
        return this.http.put<Board>(`${this.baseUrl(projectId)}/columns/reorder`, request);
    }
}
