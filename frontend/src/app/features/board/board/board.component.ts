import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { ConfirmationService, MessageService } from 'primeng/api';
import { HttpErrorResponse } from '@angular/common/http';
import { Subscription } from 'rxjs';

import { Project, ProjectMember } from 'src/app/core/models/project.model';
import { Board, BoardColumn } from 'src/app/core/models/board.model';
import { KanbanTask, Priority } from 'src/app/core/models/task.model';
import { ReportFormat } from 'src/app/core/models/report.model';
import { ProjectService } from 'src/app/core/services/project.service';
import { BoardService } from 'src/app/core/services/board.service';
import { TaskService } from 'src/app/core/services/task.service';
import { BoardHubService } from 'src/app/core/services/board-hub.service';
import { ReportService } from 'src/app/core/services/report.service';
import { AuthService } from 'src/app/core/services/auth.service';

@Component({
    selector: 'app-board',
    templateUrl: './board.component.html'
})
export class BoardComponent implements OnInit, OnDestroy {

    projectId!: string;
    project: Project | null = null;
    board: Board | null = null;
    tasksByColumn: Record<string, KanbanTask[]> = {};
    loading = false;

    readonly priorities = Object.values(Priority);
    readonly ReportFormat = ReportFormat;
    memberOptionsList: { label: string; value: string | null }[] = [{ label: 'Sin asignar', value: null }];

    columnDialogVisible = false;
    columnForm: FormGroup;
    savingColumn = false;
    editingColumnId: string | null = null;

    taskDialogVisible = false;
    taskDialogMode: 'create' | 'edit' = 'create';
    taskForm: FormGroup;
    savingTask = false;
    private editingTask: KanbanTask | null = null;
    private targetColumnIdForNewTask: string | null = null;

    exportingPdf = false;
    exportingExcel = false;

    private subscriptions = new Subscription();

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private projectService: ProjectService,
        private boardService: BoardService,
        private taskService: TaskService,
        private hub: BoardHubService,
        private reportService: ReportService,
        public authService: AuthService,
        private fb: FormBuilder,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) {
        this.columnForm = this.fb.group({
            name: ['', Validators.required]
        });

        this.taskForm = this.fb.group({
            title: ['', Validators.required],
            description: [''],
            priority: [Priority.Medium, Validators.required],
            assigneeId: [null],
            dueDate: [null]
        });
    }

    ngOnInit(): void {
        this.projectId = this.route.snapshot.paramMap.get('projectId') as string;
        this.loadAll();
        this.connectRealtime();
    }

    ngOnDestroy(): void {
        this.subscriptions.unsubscribe();
        this.hub.disconnect();
    }

    private loadAll(): void {
        this.loading = true;
        this.projectService.getById(this.projectId).subscribe({
            next: project => {
                this.project = project;
                this.memberOptionsList = [
                    { label: 'Sin asignar', value: null },
                    ...project.members.map(m => ({ label: m.fullName, value: m.userId as string | null }))
                ];
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'No se pudo cargar el proyecto.' })
        });

        this.boardService.get(this.projectId).subscribe({
            next: board => {
                this.board = board;
                this.taskService.list(this.projectId).subscribe({
                    next: tasks => {
                        this.rebuildTasksByColumn(board, tasks);
                        this.loading = false;
                    },
                    error: () => this.loading = false
                });
            },
            error: () => {
                this.loading = false;
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'No se pudo cargar el tablero.' });
            }
        });
    }

    private rebuildTasksByColumn(board: Board, tasks: KanbanTask[]): void {
        const map: Record<string, KanbanTask[]> = {};
        for (const column of board.columns) {
            map[column.id] = [];
        }
        for (const task of tasks) {
            if (!map[task.boardColumnId]) {
                map[task.boardColumnId] = [];
            }
            map[task.boardColumnId].push(task);
        }
        for (const columnId of Object.keys(map)) {
            map[columnId].sort((a, b) => a.position - b.position);
        }
        this.tasksByColumn = map;
    }

    private connectRealtime(): void {
        this.hub.joinProject(this.projectId).catch(() =>
            this.messageService.add({ severity: 'warn', summary: 'Tiempo real no disponible', detail: 'No se pudo conectar al servidor de sincronización en vivo.' })
        );

        this.subscriptions.add(this.hub.taskCreated$.subscribe(task => this.upsertTask(task)));
        this.subscriptions.add(this.hub.taskUpdated$.subscribe(task => this.upsertTask(task)));
        this.subscriptions.add(this.hub.taskMoved$.subscribe(task => this.upsertTask(task)));
        this.subscriptions.add(this.hub.taskDeleted$.subscribe(({ taskId }) => this.removeTaskLocally(taskId)));
        this.subscriptions.add(this.hub.boardChanged$.subscribe(board => this.applyBoardChanged(board)));
    }

    private upsertTask(task: KanbanTask): void {
        for (const columnId of Object.keys(this.tasksByColumn)) {
            this.tasksByColumn[columnId] = this.tasksByColumn[columnId].filter(t => t.id !== task.id);
        }
        if (!this.tasksByColumn[task.boardColumnId]) {
            this.tasksByColumn[task.boardColumnId] = [];
        }
        this.tasksByColumn[task.boardColumnId] = [...this.tasksByColumn[task.boardColumnId], task]
            .sort((a, b) => a.position - b.position);
    }

    private removeTaskLocally(taskId: string): void {
        for (const columnId of Object.keys(this.tasksByColumn)) {
            this.tasksByColumn[columnId] = this.tasksByColumn[columnId].filter(t => t.id !== taskId);
        }
    }

    private applyBoardChanged(board: Board): void {
        this.board = board;
        const map: Record<string, KanbanTask[]> = {};
        for (const column of board.columns) {
            map[column.id] = this.tasksByColumn[column.id] ?? [];
        }
        this.tasksByColumn = map;
    }

    backToProjects(): void {
        this.router.navigateByUrl('/');
    }

    // --- Columnas ---

    openCreateColumnDialog(): void {
        this.editingColumnId = null;
        this.columnForm.reset({ name: '' });
        this.columnDialogVisible = true;
    }

    openRenameColumnDialog(column: BoardColumn): void {
        this.editingColumnId = column.id;
        this.columnForm.reset({ name: column.name });
        this.columnDialogVisible = true;
    }

    saveColumn(): void {
        if (this.columnForm.invalid) {
            this.columnForm.markAllAsTouched();
            return;
        }

        this.savingColumn = true;
        const request = this.columnForm.value;

        if (this.editingColumnId) {
            this.boardService.renameColumn(this.projectId, this.editingColumnId, request).subscribe({
                next: board => {
                    this.savingColumn = false;
                    this.columnDialogVisible = false;
                    this.applyBoardChanged(board);
                },
                error: (err: HttpErrorResponse) => this.handleColumnSaveError(err)
            });
        } else {
            this.boardService.addColumn(this.projectId, request).subscribe({
                next: () => {
                    this.savingColumn = false;
                    this.columnDialogVisible = false;
                    this.loadAll();
                },
                error: (err: HttpErrorResponse) => this.handleColumnSaveError(err)
            });
        }
    }

    private handleColumnSaveError(err: HttpErrorResponse): void {
        this.savingColumn = false;
        this.messageService.add({ severity: 'error', summary: 'No se pudo guardar la columna', detail: err.error?.message });
    }

    confirmDeleteColumn(column: BoardColumn): void {
        this.confirmationService.confirm({
            message: `¿Eliminar la columna "${column.name}"?`,
            header: 'Confirmar eliminación',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'Eliminar',
            rejectLabel: 'Cancelar',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => {
                this.boardService.deleteColumn(this.projectId, column.id).subscribe({
                    next: board => this.applyBoardChanged(board),
                    error: (err: HttpErrorResponse) => {
                        this.messageService.add({
                            severity: 'error',
                            summary: 'No se pudo eliminar la columna',
                            detail: err.error?.message ?? 'La columna debe estar vacía para poder eliminarla.'
                        });
                    }
                });
            }
        });
    }

    moveColumn(column: BoardColumn, direction: -1 | 1): void {
        if (!this.board) {
            return;
        }
        const columns = [...this.board.columns].sort((a, b) => a.order - b.order);
        const index = columns.findIndex(c => c.id === column.id);
        const newIndex = index + direction;
        if (newIndex < 0 || newIndex >= columns.length) {
            return;
        }
        [columns[index], columns[newIndex]] = [columns[newIndex], columns[index]];

        this.boardService.reorderColumns(this.projectId, { orderedColumnIds: columns.map(c => c.id) }).subscribe({
            next: board => this.applyBoardChanged(board),
            error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'No se pudo reordenar la columna.' })
        });
    }

    // --- Tareas ---

    openCreateTaskDialog(columnId: string): void {
        this.taskDialogMode = 'create';
        this.editingTask = null;
        this.targetColumnIdForNewTask = columnId;
        this.taskForm.reset({ title: '', description: '', priority: Priority.Medium, assigneeId: null, dueDate: null });
        this.taskDialogVisible = true;
    }

    openEditTaskDialog(task: KanbanTask): void {
        this.taskDialogMode = 'edit';
        this.editingTask = task;
        this.taskForm.reset({
            title: task.title,
            description: task.description ?? '',
            priority: task.priority,
            assigneeId: task.assigneeId,
            dueDate: task.dueDate ? new Date(task.dueDate) : null
        });
        this.taskDialogVisible = true;
    }

    saveTask(): void {
        if (this.taskForm.invalid) {
            this.taskForm.markAllAsTouched();
            return;
        }

        this.savingTask = true;
        const value = this.taskForm.value;
        const dueDate = value.dueDate ? (value.dueDate as Date).toISOString() : null;

        if (this.taskDialogMode === 'create') {
            this.taskService.create(this.projectId, {
                boardColumnId: this.targetColumnIdForNewTask as string,
                title: value.title,
                description: value.description || null,
                priority: value.priority,
                assigneeId: value.assigneeId,
                dueDate
            }).subscribe({
                next: task => {
                    this.savingTask = false;
                    this.taskDialogVisible = false;
                    this.upsertTask(task);
                },
                error: (err: HttpErrorResponse) => this.handleTaskSaveError(err)
            });
        } else if (this.editingTask) {
            this.taskService.update(this.projectId, this.editingTask.id, {
                title: value.title,
                description: value.description || null,
                priority: value.priority,
                assigneeId: value.assigneeId,
                dueDate
            }).subscribe({
                next: task => {
                    this.savingTask = false;
                    this.taskDialogVisible = false;
                    this.upsertTask(task);
                },
                error: (err: HttpErrorResponse) => this.handleTaskSaveError(err)
            });
        }
    }

    private handleTaskSaveError(err: HttpErrorResponse): void {
        this.savingTask = false;
        this.messageService.add({ severity: 'error', summary: 'No se pudo guardar la tarea', detail: err.error?.message });
    }

    confirmDeleteTask(task: KanbanTask): void {
        this.confirmationService.confirm({
            message: `¿Eliminar la tarea "${task.title}"?`,
            header: 'Confirmar eliminación',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'Eliminar',
            rejectLabel: 'Cancelar',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => {
                this.taskService.delete(this.projectId, task.id).subscribe({
                    next: () => this.removeTaskLocally(task.id),
                    error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'No se pudo eliminar la tarea.' })
                });
            }
        });
    }

    columnIds(): string[] {
        return this.board ? this.board.columns.sort((a, b) => a.order - b.order).map(c => c.id) : [];
    }

    priorityColor(priority: Priority): 'success' | 'info' | 'warning' | 'danger' {
        switch (priority) {
            case Priority.Low: return 'success';
            case Priority.Medium: return 'info';
            case Priority.High: return 'warning';
            case Priority.Critical: return 'danger';
        }
    }

    priorityLabel(priority: Priority): string {
        switch (priority) {
            case Priority.Low: return 'Baja';
            case Priority.Medium: return 'Media';
            case Priority.High: return 'Alta';
            case Priority.Critical: return 'Crítica';
        }
    }

    memberName(userId: string | null): string | null {
        if (!userId || !this.project) {
            return null;
        }
        return this.project.members.find(m => m.userId === userId)?.fullName ?? null;
    }

    drop(event: CdkDragDrop<KanbanTask[]>, targetColumnId: string): void {
        if (event.previousContainer === event.container) {
            if (event.previousIndex === event.currentIndex) {
                return;
            }
            moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
        } else {
            transferArrayItem(event.previousContainer.data, event.container.data, event.previousIndex, event.currentIndex);
        }

        const list = event.container.data;
        const movedTask = list[event.currentIndex];
        const previousTaskId = event.currentIndex > 0 ? list[event.currentIndex - 1].id : null;
        const nextTaskId = event.currentIndex < list.length - 1 ? list[event.currentIndex + 1].id : null;

        this.taskService.move(this.projectId, movedTask.id, {
            targetColumnId,
            previousTaskId,
            nextTaskId
        }).subscribe({
            next: task => this.upsertTask(task),
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'No se pudo mover la tarea, se restauró el tablero.' });
                this.loadAll();
            }
        });
    }

    // --- Exportación ---

    exportReport(format: ReportFormat): void {
        const isPdf = format === ReportFormat.Pdf;
        if (isPdf) {
            this.exportingPdf = true;
        } else {
            this.exportingExcel = true;
        }

        this.reportService.export(this.projectId, format).subscribe({
            next: blob => {
                this.exportingPdf = false;
                this.exportingExcel = false;
                const extension = isPdf ? 'pdf' : 'xlsx';
                const safeName = (this.project?.name ?? 'reporte').replace(/[^a-z0-9]+/gi, '-').toLowerCase();
                this.downloadBlob(blob, `${safeName}.${extension}`);
            },
            error: () => {
                this.exportingPdf = false;
                this.exportingExcel = false;
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'No se pudo generar el reporte.' });
            }
        });
    }

    private downloadBlob(blob: Blob, fileName: string): void {
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = fileName;
        anchor.click();
        URL.revokeObjectURL(url);
    }
}
