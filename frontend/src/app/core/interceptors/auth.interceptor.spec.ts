import { TestBed } from '@angular/core/testing';
import { HttpClient, HTTP_INTERCEPTORS } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthInterceptor } from './auth.interceptor';
import { AuthService } from '../services/auth.service';

describe('AuthInterceptor', () => {
    let http: HttpClient;
    let httpMock: HttpTestingController;
    let authServiceSpy: jasmine.SpyObj<AuthService>;

    beforeEach(() => {
        authServiceSpy = jasmine.createSpyObj('AuthService', [], { token: null });

        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [
                { provide: AuthService, useValue: authServiceSpy },
                { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
            ]
        });

        http = TestBed.inject(HttpClient);
        httpMock = TestBed.inject(HttpTestingController);
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
});
