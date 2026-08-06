import { Injectable } from '@angular/core';
import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

    constructor(private authService: AuthService, private router: Router) { }

    intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
        const token = this.authService.token;
        if (token) {
            request = request.clone({
                setHeaders: { Authorization: `Bearer ${token}` }
            });
        }

        return next.handle(request).pipe(
            catchError((error: HttpErrorResponse) => {
                // Solo forzamos logout si la peticion iba autenticada y el servidor la rechazo
                // (sesion vencida/token invalido). Un 401 de /auth/login o /auth/register (credenciales
                // incorrectas) no lleva token, asi que no dispara esto: lo maneja el propio componente.
                if (error.status === 401 && token) {
                    this.authService.logout();
                    this.router.navigate(['/auth/login'], { queryParams: { returnUrl: this.router.url } });
                }
                return throwError(() => error);
            })
        );
    }
}
