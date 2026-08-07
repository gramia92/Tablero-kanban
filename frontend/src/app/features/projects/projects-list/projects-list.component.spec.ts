import { FormBuilder } from '@angular/forms';
import { of } from 'rxjs';
import { ProjectsListComponent } from './projects-list.component';
import { Project } from 'src/app/core/models/project.model';

describe('ProjectsListComponent', () => {
    let component: ProjectsListComponent;
    let projectServiceSpy: jasmine.SpyObj<any>;
    let authServiceStub: any;

    const project: Project = {
        id: 'p1',
        name: 'Proyecto Kanban',
        description: null,
        ownerId: 'owner-1',
        members: [],
        createdAtUtc: '2026-01-01T00:00:00Z'
    };

    beforeEach(() => {
        projectServiceSpy = jasmine.createSpyObj('ProjectService', ['list', 'create', 'update', 'delete', 'addMember', 'removeMember', 'getById']);
        projectServiceSpy.list.and.returnValue(of({ items: [], totalCount: 0, page: 1, pageSize: 9 }));
        authServiceStub = { currentUser: { userId: 'owner-1' } };

        component = new ProjectsListComponent(
            projectServiceSpy,
            authServiceStub,
            new FormBuilder(),
            jasmine.createSpyObj('MessageService', ['add']),
            jasmine.createSpyObj('ConfirmationService', ['confirm'])
        );
    });

    it('isOwner() should return true when the current user owns the project', () => {
        expect(component.isOwner(project)).toBeTrue();
    });

    it('isOwner() should return false when the current user is a different member', () => {
        authServiceStub.currentUser = { userId: 'someone-else' };

        expect(component.isOwner(project)).toBeFalse();
    });

    it('isOwner() should return false when there is no logged-in user', () => {
        authServiceStub.currentUser = null;

        expect(component.isOwner(project)).toBeFalse();
    });

    it('loadProjects() should populate the projects list and total count from the service', () => {
        projectServiceSpy.list.and.returnValue(of({ items: [project], totalCount: 1, page: 1, pageSize: 9 }));

        component.loadProjects();

        expect(component.projects).toEqual([project]);
        expect(component.totalCount).toBe(1);
        expect(component.loading).toBeFalse();
    });

    it('onSearchInput() should debounce and reload from page 1 with the search term', (done) => {
        projectServiceSpy.list.and.returnValue(of({ items: [], totalCount: 0, page: 1, pageSize: 9 }));
        component.page = 3;

        component.onSearchInput('kanban');

        setTimeout(() => {
            expect(component.page).toBe(1);
            expect(component.searchTerm).toBe('kanban');
            expect(projectServiceSpy.list).toHaveBeenCalledWith(1, 9, 'kanban');
            done();
        }, 450);
    });
});
