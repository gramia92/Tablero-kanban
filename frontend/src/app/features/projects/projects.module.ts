import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToolbarModule } from 'primeng/toolbar';
import { PaginatorModule } from 'primeng/paginator';
import { ConfirmationService } from 'primeng/api';
import { ProjectsRoutingModule } from './projects-routing.module';
import { ProjectsListComponent } from './projects-list/projects-list.component';

@NgModule({
    imports: [
        CommonModule,
        RouterModule,
        ReactiveFormsModule,
        ProjectsRoutingModule,
        ButtonModule,
        CardModule,
        DialogModule,
        InputTextModule,
        InputTextareaModule,
        TableModule,
        TagModule,
        ConfirmDialogModule,
        ToolbarModule,
        PaginatorModule
    ],
    declarations: [ProjectsListComponent],
    providers: [ConfirmationService]
})
export class ProjectsModule { }
