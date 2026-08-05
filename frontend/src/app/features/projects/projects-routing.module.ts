import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ProjectsListComponent } from './projects-list/projects-list.component';

@NgModule({
    imports: [RouterModule.forChild([
        { path: '', component: ProjectsListComponent }
    ])],
    exports: [RouterModule]
})
export class ProjectsRoutingModule { }
