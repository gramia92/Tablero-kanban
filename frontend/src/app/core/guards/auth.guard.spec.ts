import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { AuthGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';

describe('AuthGuard', () => {
    let guard: AuthGuard;
    let authServiceSpy: jasmine.SpyObj<AuthService>;
    let routerSpy: jasmine.SpyObj<Router>;

    beforeEach(() => {
        authServiceSpy = jasmine.createSpyObj('AuthService', ['isAuthenticated']);
        routerSpy = jasmine.createSpyObj('Router', ['createUrlTree']);
        guard = new AuthGuard(authServiceSpy, routerSpy);
    });

    it('should allow navigation when the user is authenticated', () => {
        authServiceSpy.isAuthenticated.and.returnValue(true);

        const result = guard.canActivate(
            {} as ActivatedRouteSnapshot,
            { url: '/board/123' } as RouterStateSnapshot
        );

        expect(result).toBeTrue();
        expect(routerSpy.createUrlTree).not.toHaveBeenCalled();
    });

    it('should redirect to /auth/login with a returnUrl when the user is not authenticated', () => {
        authServiceSpy.isAuthenticated.and.returnValue(false);
        const fakeUrlTree = {} as UrlTree;
        routerSpy.createUrlTree.and.returnValue(fakeUrlTree);

        const result = guard.canActivate(
            {} as ActivatedRouteSnapshot,
            { url: '/board/123' } as RouterStateSnapshot
        );

        expect(result).toBe(fakeUrlTree);
        expect(routerSpy.createUrlTree).toHaveBeenCalledWith(['/auth/login'], { queryParams: { returnUrl: '/board/123' } });
    });
});
