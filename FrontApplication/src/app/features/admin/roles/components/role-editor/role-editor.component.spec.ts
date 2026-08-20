import { RoleEditorComponent } from './role-editor.component';

describe('RoleEditorComponent', () => {
  it('protects baseline and current-account admin roles', () => {
    const component = new RoleEditorComponent();
    component.selectedRoleNames = ['User', 'Admin'];
    component.protectAdminRole = true;

    expect(component.isSelected('admin')).toBe(true);
    expect(component.isLocked('User')).toBe(true);
    expect(component.isLocked('Admin')).toBe(true);
    expect(component.isLocked('Operator')).toBe(false);
  });

  it('emits an explicit role change from the reusable editor', () => {
    const component = new RoleEditorComponent();
    const changes: unknown[] = [];
    component.roleChanged.subscribe((change) => changes.push(change));

    component.changeRole('Operator', { target: { checked: true } } as unknown as Event);

    expect(changes).toEqual([{ roleName: 'Operator', enabled: true }]);
  });
});
