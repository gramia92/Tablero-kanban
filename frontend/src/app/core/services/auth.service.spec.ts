import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { AuthResponse } from '../models/auth.model';
import { environment } from 'src/environments/environment';

describe('AuthService', () => {
    let service: AuthService;
    let httpMock: HttpTestingController;

    const fakeResponse: AuthResponse = {
        accessToken: 'fake-token',
        expiresAtUtc: '2030-01-01T00:00:00Z',
        userId: 'user-1',
        fullName: 'Gaby Ramirez',
        email: 'gaby@example.com'
    };

    beforeEach(() => {
        localStorage.removeItem('kanban_auth');
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [AuthService]
        });
        service = TestBed.inject(AuthService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
        localStorage.removeItem('kanban_auth');
    });

    it('should start unauthenticated when localStorage is empty', () => {
        expect(service.isAuthenticated()).toBeFalse();
        expect(service.token).toBeNull();
    });

    it('login() should store the session and mark the user as authenticated', () => {
        service.login({ email: fakeResponse.email, password: 'Passw0rd!' }).subscribe(response => {
            expect(response).toEqual(fakeResponse);
        });

        const req = httpMock.expectOne(`${environment.apiUrl}/auth/login`);
        expect(req.request.method).toBe('POST');
        req.flush(fakeResponse);

        expect(service.isAuthenticated()).toBeTrue();
        expect(service.token).toBe('fake-token');
        expect(JSON.parse(localStorage.getItem('kanban_auth') as string)).toEqual(fakeResponse);
    });

    it('logout() should clear the session and localStorage', () => {
        service.login({ email: fakeResponse.email, password: 'Passw0rd!' }).subscribe();
        httpMock.expectOne(`${environment.apiUrl}/auth/login`).flush(fakeResponse);
        expect(service.isAuthenticated()).toBeTrue();

        service.logout();

        expect(service.isAuthenticated()).toBeFalse();
        expect(service.token).toBeNull();
        expect(localStorage.getItem('kanban_auth')).toBeNull();
    });

    it('should restore an existing session from localStorage on construction', () => {
        localStorage.setItem('kanban_auth', JSON.stringify(fakeResponse));

        TestBed.resetTestingModule();
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [AuthService]
        });
        const restored = TestBed.inject(AuthService);

        expect(restored.isAuthenticated()).toBeTrue();
        expect(restored.currentUser?.fullName).toBe('Gaby Ramirez');
    });
});
