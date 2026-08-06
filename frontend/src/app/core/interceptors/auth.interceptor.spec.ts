import { TestBed } from '@angular/core/testing';
import { HttpClient, HTTP_INTERCEPTORS } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { AuthInterceptor } from './auth.interceptor';
import { AuthService } from '../services/auth.service';

describe('AuthInterceptor', () => {
    let http: HttpClient;
    let httpMock: HttpTestingController;
    let authServiceSpy: jasmine.SpyObj<AuthService>;
    let router: Router;

    beforeEach(() => {
        authServiceSpy = jasmine.createSpyObj('AuthService', ['logout'], { token: null });

        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule, RouterTestingModule],
            providers: [
                { provide: AuthService, useValue: authServiceSpy },
                { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
            ]
        });

        http = TestBed.inject(HttpClient);
        httpMock = TestBed.inject(HttpTestingController);
        router = TestBed.inject(Router);
        spyOn(router, 'navigate');
    });

    afterEach(() => httpMock.verify());

    it('should add an Authorization header when a token exists', () => {
        Object.defineProperty(authServiceSpy, 'token', { get: () => 'my-token' });

        http.get('/api/projects').subscribe();

        const req = httpMock.expectOne('/api/projects');
        expect(req.request.headers.get('Authorization')).toBe('Bearer my-token');
        req.flush({});
    });

    it('should not add an Authorization header when there is no token', () => {
        Object.defineProperty(authServiceSpy, 'token', { get: () => null });

        http.get('/api/projects').subscribe();

        const req = httpMock.expectOne('/api/projects');
        expect(req.request.headers.has('Authorization')).toBeFalse();
        req.flush({});
    });

    it('should log out and redirect to login when an authenticated request gets a 401 (expired session)', () => {
        Object.defineProperty(authServiceSpy, 'token', { get: () => 'my-token' });

        http.get('/api/projects').subscribe({ error: () => { } });

        const req = httpMock.expectOne('/api/projects');
        req.flush({ message: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });

        expect(authServiceSpy.logout).toHaveBeenCalled();
        expect(router.navigate).toHaveBeenCalledWith(['/auth/login'], jasmine.any(Object));
    });

    it('should NOT log out on a 401 from an unauthenticated request (e.g. wrong login credentials)', () => {
        Object.defineProperty(authServiceSpy, 'token', { get: () => null });

        http.post('/api/auth/login', {}).subscribe({ error: () => { } });

        const req = httpMock.expectOne('/api/auth/login');
        req.flush({ message: 'Credenciales invalidas' }, { status: 401, statusText: 'Unauthorized' });

        expect(authServiceSpy.logout).not.toHaveBeenCalled();
        expect(router.navigate).not.toHaveBeenCalled();
    });
});
