import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { HttpErrorResponse } from '@angular/common/http';
import { Project, ProjectMember } from 'src/app/core/models/project.model';
import { ProjectService } from 'src/app/core/services/project.service';
import { AuthService } from 'src/app/core/services/auth.service';

@Component({
    selector: 'app-projects-list',
    templateUrl: './projects-list.component.html'
})
export class ProjectsListComponent implements OnInit {

    projects: Project[] = [];
    loading = false;

    projectDialogVisible = false;
    projectDialogMode: 'create' | 'edit' = 'create';
    projectForm: FormGroup;
    savingProject = false;
    private editingProjectId: string | null = null;

    membersDialogVisible = false;
    membersDialogProject: Project | null = null;
    memberEmailForm: FormGroup;
    savingMember = false;

    constructor(
        private projectService: ProjectService,
        public authService: AuthService,
        private fb: FormBuilder,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) {
        this.projectForm = this.fb.group({
            name: ['', Validators.required],
            description: ['']
        });
        this.memberEmailForm = this.fb.group({
            email: ['', [Validators.required, Validators.email]]
        });
    }

    ngOnInit(): void {
        this.loadProjects();
    }

    loadProjects(): void {
        this.loading = true;
        this.projectService.list().subscribe({
            next: projects => {
                this.projects = projects;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'No se pudieron cargar los proyectos.' });
            }
        });
    }

    isOwner(project: Project): boolean {
        return project.ownerId === this.authService.currentUser?.userId;
    }

    openCreateDialog(): void {
        this.projectDialogMode = 'create';
        this.editingProjectId = null;
        this.projectForm.reset({ name: '', description: '' });
        this.projectDialogVisible = true;
    }

    openEditDialog(project: Project): void {
        this.projectDialogMode = 'edit';
        this.editingProjectId = project.id;
        this.projectForm.reset({ name: project.name, description: project.description ?? '' });
        this.projectDialogVisible = true;
    }

    saveProject(): void {
        if (this.projectForm.invalid) {
            this.projectForm.markAllAsTouched();
            return;
        }

        this.savingProject = true;
        const request = this.projectForm.value;
        const operation = this.projectDialogMode === 'create'
            ? this.projectService.create(request)
            : this.projectService.update(this.editingProjectId as string, request);

        operation.subscribe({
            next: () => {
                this.savingProject = false;
                this.projectDialogVisible = false;
                this.messageService.add({
                    severity: 'success',
                    summary: this.projectDialogMode === 'create' ? 'Proyecto creado' : 'Proyecto actualizado',
                    detail: request.name
                });
                this.loadProjects();
            },
            error: (err: HttpErrorResponse) => {
                this.savingProject = false;
                this.messageService.add({
                    severity: 'error',
                    summary: 'No se pudo guardar el proyecto',
                    detail: err.error?.message ?? 'Intenta de nuevo en unos segundos.'
                });
            }
        });
    }

    confirmDelete(project: Project): void {
        this.confirmationService.confirm({
            message: `¿Eliminar el proyecto "${project.name}"? Esta acción no se puede deshacer.`,
            header: 'Confirmar eliminación',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'Eliminar',
            rejectLabel: 'Cancelar',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => this.deleteProject(project)
        });
    }

    private deleteProject(project: Project): void {
        this.projectService.delete(project.id).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: 'Proyecto eliminado', detail: project.name });
                this.loadProjects();
            },
            error: (err: HttpErrorResponse) => {
                this.messageService.add({
                    severity: 'error',
                    summary: 'No se pudo eliminar el proyecto',
                    detail: err.error?.message ?? 'Intenta de nuevo en unos segundos.'
                });
            }
        });
    }

    openMembersDialog(project: Project): void {
        this.membersDialogProject = project;
        this.memberEmailForm.reset({ email: '' });
        this.membersDialogVisible = true;
    }

    addMember(): void {
        if (this.memberEmailForm.invalid || !this.membersDialogProject) {
            this.memberEmailForm.markAllAsTouched();
            return;
        }

        this.savingMember = true;
        const projectId = this.membersDialogProject.id;
        this.projectService.addMember(projectId, this.memberEmailForm.value).subscribe({
            next: () => {
                this.savingMember = false;
                this.memberEmailForm.reset({ email: '' });
                this.refreshMembersDialog(projectId);
                this.loadProjects();
            },
            error: (err: HttpErrorResponse) => {
                this.savingMember = false;
                this.messageService.add({
                    severity: 'error',
                    summary: 'No se pudo agregar el miembro',
                    detail: err.error?.message ?? 'Verifica que el email esté registrado.'
                });
            }
        });
    }

    removeMember(member: ProjectMember): void {
        if (!this.membersDialogProject) {
            return;
        }

        const projectId = this.membersDialogProject.id;
        this.projectService.removeMember(projectId, member.userId).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: 'Miembro eliminado', detail: member.fullName });
                this.refreshMembersDialog(projectId);
                this.loadProjects();
            },
            error: (err: HttpErrorResponse) => {
                this.messageService.add({
                    severity: 'error',
                    summary: 'No se pudo eliminar el miembro',
                    detail: err.error?.message ?? 'Intenta de nuevo en unos segundos.'
                });
            }
        });
    }

    private refreshMembersDialog(projectId: string): void {
        this.projectService.getById(projectId).subscribe(project => {
            this.membersDialogProject = project;
            const updated = this.projects.find(p => p.id === projectId);
            if (updated) {
                Object.assign(updated, project);
            }
        });
    }
}
