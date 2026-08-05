import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { BoardComponent } from './board/board.component';

@NgModule({
    imports: [RouterModule.forChild([
        { path: '', component: BoardComponent }
    ])],
    exports: [RouterModule]
})
export class BoardRoutingModule { }
