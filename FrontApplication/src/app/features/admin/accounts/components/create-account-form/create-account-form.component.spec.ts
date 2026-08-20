import { TestBed } from '@angular/core/testing';

import { CreateAccountFormComponent } from './create-account-form.component';

describe('CreateAccountFormComponent', () => {
  it('returns only valid account creation requests and resets its state', () => {
    TestBed.configureTestingModule({});
    const component = TestBed.runInInjectionContext(() => new CreateAccountFormComponent());

    expect(component.request()).toBeNull();

    component.form.setValue({
      username: 'tenant.user',
      email: 'tenant@example.com',
      temporaryPassword: 'Temporary-123',
      sendInvite: true,
    });

    expect(component.request()).toEqual({
      username: 'tenant.user',
      email: 'tenant@example.com',
      temporaryPassword: 'Temporary-123',
      sendInvite: true,
    });

    component.reset();

    expect(component.invalid).toBe(true);
    expect(component.form.controls.username.value).toBe('');
  });
});
