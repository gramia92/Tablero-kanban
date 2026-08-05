import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { AuthService } from './auth.service';
import { KanbanTask } from '../models/task.model';
import { Board } from '../models/board.model';

@Injectable({ providedIn: 'root' })
export class BoardHubService {

    private connection: signalR.HubConnection | null = null;
    private currentProjectId: string | null = null;

    readonly taskCreated$ = new Subject<KanbanTask>();
    readonly taskUpdated$ = new Subject<KanbanTask>();
    readonly taskMoved$ = new Subject<KanbanTask>();
    readonly taskDeleted$ = new Subject<{ taskId: string }>();
    readonly boardChanged$ = new Subject<Board>();

    constructor(private authService: AuthService) { }

    async joinProject(projectId: string): Promise<void> {
        await this.ensureConnected();
        this.currentProjectId = projectId;
        await this.connection!.invoke('JoinProject', projectId);
    }

    async leaveCurrentProject(): Promise<void> {
        if (this.connection && this.currentProjectId) {
            try {
                await this.connection.invoke('LeaveProject', this.currentProjectId);
            } catch {
                // la conexión pudo haberse cerrado ya; no bloquear la navegación por esto
            }
        }
        this.currentProjectId = null;
    }

    async disconnect(): Promise<void> {
        await this.leaveCurrentProject();
        if (this.connection) {
            await this.connection.stop();
            this.connection = null;
        }
    }

    private async ensureConnected(): Promise<void> {
        if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
            return;
        }

        if (!this.connection) {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl(environment.hubUrl, { accessTokenFactory: () => this.authService.token ?? '' })
                .withAutomaticReconnect()
                .build();

            this.connection.on('TaskCreated', (task: KanbanTask) => this.taskCreated$.next(task));
            this.connection.on('TaskUpdated', (task: KanbanTask) => this.taskUpdated$.next(task));
            this.connection.on('TaskMoved', (task: KanbanTask) => this.taskMoved$.next(task));
            this.connection.on('TaskDeleted', (payload: { taskId: string }) => this.taskDeleted$.next(payload));
            this.connection.on('BoardChanged', (board: Board) => this.boardChanged$.next(board));

            this.connection.onreconnected(async () => {
                if (this.currentProjectId) {
                    await this.connection!.invoke('JoinProject', this.currentProjectId);
                }
            });
        }

        if (this.connection.state === signalR.HubConnectionState.Disconnected) {
            await this.connection.start();
        }
    }
}
