import { FormBuilder } from '@angular/forms';
import { of } from 'rxjs';
import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { BoardComponent } from './board.component';
import { KanbanTask, Priority } from 'src/app/core/models/task.model';

function makeTask(id: string, boardColumnId: string, position: number): KanbanTask {
    return {
        id,
        boardColumnId,
        title: `Tarea ${id}`,
        description: null,
        priority: Priority.Medium,
        assigneeId: null,
        assigneeName: null,
        dueDate: null,
        position
    } as KanbanTask;
}

describe('BoardComponent', () => {
    let component: BoardComponent;
    let taskServiceSpy: jasmine.SpyObj<any>;

    beforeEach(() => {
        taskServiceSpy = jasmine.createSpyObj('TaskService', ['move', 'create', 'update', 'delete', 'list']);

        component = new BoardComponent(
            { snapshot: { paramMap: { get: () => 'project-1' } } } as any,
            jasmine.createSpyObj('Router', ['navigateByUrl']),
            jasmine.createSpyObj('ProjectService', ['getById']),
            jasmine.createSpyObj('BoardService', ['get', 'addColumn', 'renameColumn', 'deleteColumn', 'reorderColumns']),
            taskServiceSpy,
            jasmine.createSpyObj('BoardHubService', ['joinProject', 'leaveCurrentProject', 'disconnect'], {
                taskCreated$: of(), taskUpdated$: of(), taskMoved$: of(), taskDeleted$: of(), boardChanged$: of()
            }),
            jasmine.createSpyObj('ReportService', ['export']),
            {} as any,
            new FormBuilder(),
            jasmine.createSpyObj('MessageService', ['add']),
            jasmine.createSpyObj('ConfirmationService', ['confirm'])
        );

        component.projectId = 'project-1';
    });

    it('priorityLabel() should translate each priority to Spanish', () => {
        expect(component.priorityLabel(Priority.Low)).toBe('Baja');
        expect(component.priorityLabel(Priority.Medium)).toBe('Media');
        expect(component.priorityLabel(Priority.High)).toBe('Alta');
        expect(component.priorityLabel(Priority.Critical)).toBe('Crítica');
    });

    it('priorityColor() should map each priority to its PrimeNG severity', () => {
        expect(component.priorityColor(Priority.Low)).toBe('success');
        expect(component.priorityColor(Priority.Critical)).toBe('danger');
    });

    it('drop() reordering within the same column should send the new neighbor ids', () => {
        const a = makeTask('a', 'col-1', 65536);
        const b = makeTask('b', 'col-1', 131072);
        const c = makeTask('c', 'col-1', 196608);
        const columnData = [a, b, c];
        const container = { data: columnData };
        taskServiceSpy.move.and.returnValue(of(a));

        const event = {
            previousContainer: container,
            container: container,
            previousIndex: 0,
            currentIndex: 2
        } as unknown as CdkDragDrop<KanbanTask[]>;

        component.drop(event, 'col-1');

        // a moved from index 0 to index 2: order is now [b, c, a]
        expect(taskServiceSpy.move).toHaveBeenCalledWith('project-1', 'a', {
            targetColumnId: 'col-1',
            previousTaskId: 'c',
            nextTaskId: null
        });
    });

    it('drop() moving a task into an empty column should send null neighbors', () => {
        const moved = makeTask('x', 'col-1', 65536);
        const sourceData = [moved];
        const targetData: KanbanTask[] = [];
        taskServiceSpy.move.and.returnValue(of(moved));

        const event = {
            previousContainer: { data: sourceData },
            container: { data: targetData },
            previousIndex: 0,
            currentIndex: 0
        } as unknown as CdkDragDrop<KanbanTask[]>;

        component.drop(event, 'col-2');

        expect(taskServiceSpy.move).toHaveBeenCalledWith('project-1', 'x', {
            targetColumnId: 'col-2',
            previousTaskId: null,
            nextTaskId: null
        });
    });

    it('drop() should do nothing when a task is dropped back in its original spot', () => {
        const a = makeTask('a', 'col-1', 65536);
        const columnData = [a];
        const container = { data: columnData };

        const event = {
            previousContainer: container,
            container: container,
            previousIndex: 0,
            currentIndex: 0
        } as unknown as CdkDragDrop<KanbanTask[]>;

        component.drop(event, 'col-1');

        expect(taskServiceSpy.move).not.toHaveBeenCalled();
    });
});
