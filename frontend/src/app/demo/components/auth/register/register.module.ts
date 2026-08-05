import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { RegisterRoutingModule } from './register-routing.module';
import { RegisterComponent } from './register.component';

@NgModule({
    imports: [
        CommonModule,
        RegisterRoutingModule,
        RouterModule,
        ButtonModule,
        InputTextModule,
        ReactiveFormsModule,
        PasswordModule
    ],
    declarations: [RegisterComponent]
})
export class RegisterModule { }
