import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from 'src/environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/auth.model';

const STORAGE_KEY = 'kanban_auth';

@Injectable({ providedIn: 'root' })
export class AuthService {

    private currentUserSubject: BehaviorSubject<AuthResponse | null>;
    currentUser$: Observable<AuthResponse | null>;

    constructor(private http: HttpClient) {
        this.currentUserSubject = new BehaviorSubject<AuthResponse | null>(this.readFromStorage());
        this.currentUser$ = this.currentUserSubject.asObservable();
    }

    get currentUser(): AuthResponse | null {
        return this.currentUserSubject.value;
    }

    get token(): string | null {
        return this.currentUserSubject.value?.accessToken ?? null;
    }

    isAuthenticated(): boolean {
        return !!this.currentUser;
    }

    login(request: LoginRequest): Observable<AuthResponse> {
        return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, request)
            .pipe(tap(response => this.setSession(response)));
    }

    register(request: RegisterRequest): Observable<AuthResponse> {
        return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/register`, request)
            .pipe(tap(response => this.setSession(response)));
    }

    logout(): void {
        localStorage.removeItem(STORAGE_KEY);
        this.currentUserSubject.next(null);
    }

    private setSession(response: AuthResponse): void {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(response));
        this.currentUserSubject.next(response);
    }

    private readFromStorage(): AuthResponse | null {
        const raw = localStorage.getItem(STORAGE_KEY);
        if (!raw) {
            return null;
        }
        try {
            return JSON.parse(raw) as AuthResponse;
        } catch {
            return null;
        }
    }
}
