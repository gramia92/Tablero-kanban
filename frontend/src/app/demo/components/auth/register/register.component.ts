import { Component } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { HttpErrorResponse } from '@angular/common/http';
import { LayoutService } from 'src/app/layout/service/app.layout.service';
import { AuthService } from 'src/app/core/services/auth.service';

function passwordsMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;
    return password && confirmPassword && password !== confirmPassword ? { passwordMismatch: true } : null;
}

@Component({
    selector: 'app-register',
    templateUrl: './register.component.html',
    styles: [`
        :host ::ng-deep .pi-eye,
        :host ::ng-deep .pi-eye-slash {
            transform:scale(1.6);
            margin-right: 1rem;
            color: var(--primary-color) !important;
        }
    `]
})
export class RegisterComponent {

    form: FormGroup;

    loading = false;

    constructor(
        public layoutService: LayoutService,
        private fb: FormBuilder,
        private authService: AuthService,
        private router: Router,
        private messageService: MessageService
    ) {
        this.form = this.fb.group({
            fullName: ['', Validators.required],
            email: ['', [Validators.required, Validators.email]],
            password: ['', [Validators.required, Validators.minLength(6)]],
            confirmPassword: ['', Validators.required]
        }, { validators: passwordsMatchValidator });
    }

    submit(): void {
        if (this.form.invalid) {
            this.form.markAllAsTouched();
            return;
        }

        this.loading = true;
        const { fullName, email, password } = this.form.value;
        this.authService.register({ fullName, email, password }).subscribe({
            next: () => {
                this.loading = false;
                this.router.navigateByUrl('/');
            },
            error: (err: HttpErrorResponse) => {
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: 'No se pudo crear la cuenta',
                    detail: err.error?.message ?? 'Intenta de nuevo en unos segundos.'
                });
            }
        });
    }
}
