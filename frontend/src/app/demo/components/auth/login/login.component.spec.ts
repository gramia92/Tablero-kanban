import { FormBuilder } from '@angular/forms';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { LoginComponent } from './login.component';
import { AuthService } from 'src/app/core/services/auth.service';

describe('LoginComponent', () => {
    let component: LoginComponent;
    let authServiceSpy: jasmine.SpyObj<AuthService>;
    let routerSpy: jasmine.SpyObj<any>;
    let messageServiceSpy: jasmine.SpyObj<any>;
    let route: any;

    beforeEach(() => {
        authServiceSpy = jasmine.createSpyObj('AuthService', ['login']);
        routerSpy = jasmine.createSpyObj('Router', ['navigateByUrl']);
        messageServiceSpy = jasmine.createSpyObj('MessageService', ['add']);
        route = { snapshot: { queryParamMap: { get: () => null } } };

        component = new LoginComponent(
            {} as any,
            new FormBuilder(),
            authServiceSpy,
            routerSpy,
            route,
            messageServiceSpy
        );
    });

    it('should not call authService.login when the form is invalid', () => {
        component.form.patchValue({ email: '', password: '' });

        component.submit();

        expect(authServiceSpy.login).not.toHaveBeenCalled();
        expect(component.form.get('email')?.touched).toBeTrue();
    });

    it('should log in and navigate to the returnUrl on success', () => {
        route.snapshot.queryParamMap.get = () => '/board/123';
        authServiceSpy.login.and.returnValue(of({
            accessToken: 't', expiresAtUtc: '', userId: 'u1', fullName: 'Gaby', email: 'gaby@example.com'
        }));
        component.form.setValue({ email: 'gaby@example.com', password: 'Passw0rd!' });

        component.submit();

        expect(authServiceSpy.login).toHaveBeenCalledWith({ email: 'gaby@example.com', password: 'Passw0rd!' });
        expect(routerSpy.navigateByUrl).toHaveBeenCalledWith('/board/123');
        expect(component.loading).toBeFalse();
    });

    it('should show an error toast when login fails', () => {
        authServiceSpy.login.and.returnValue(throwError(() => new HttpErrorResponse({ error: { message: 'Credenciales inválidas' } })));
        component.form.setValue({ email: 'gaby@example.com', password: 'wrong' });

        component.submit();

        expect(messageServiceSpy.add).toHaveBeenCalledWith(jasmine.objectContaining({
            severity: 'error',
            detail: 'Credenciales inválidas'
        }));
        expect(component.loading).toBeFalse();
        expect(routerSpy.navigateByUrl).not.toHaveBeenCalled();
    });
});
