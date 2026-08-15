import { ConfirmationDialogComponent } from './confirmation-dialog.component';

describe('ConfirmationDialogComponent', () => {
  it('runs the confirmation action and closes after success', async () => {
    const component = new ConfirmationDialogComponent();
    const action = vi.fn(() => Promise.resolve());
    const onSuccess = vi.fn();
    const openChanges: boolean[] = [];
    const confirmations: void[] = [];
    component.confirmAction = action;
    component.onSuccess = onSuccess;
    component.openChange.subscribe((open) => openChanges.push(open));
    component.confirmed.subscribe(() => confirmations.push(undefined));

    component.confirm();
    await flushPromises();

    expect(action).toHaveBeenCalledOnce();
    expect(onSuccess).toHaveBeenCalledOnce();
    expect(confirmations.length).toBe(1);
    expect(openChanges).toEqual([false]);
    expect(component.processing).toBe(false);
  });

  it('keeps the dialog open and reports failures when the action fails', () => {
    const component = new ConfirmationDialogComponent();
    const error = new Error('Operation failed');
    const onFailure = vi.fn();
    const openChanges: boolean[] = [];
    const failures: unknown[] = [];
    component.confirmAction = () => {
      throw error;
    };
    component.onFailure = onFailure;
    component.openChange.subscribe((open) => openChanges.push(open));
    component.failed.subscribe((failure) => failures.push(failure));

    component.confirm();

    expect(onFailure).toHaveBeenCalledWith(error);
    expect(failures).toEqual([error]);
    expect(openChanges).toEqual([]);
    expect(component.failureMessage).toBe(error.message);
    expect(component.processing).toBe(false);
  });
});

async function flushPromises(): Promise<void> {
  await Promise.resolve();
  await Promise.resolve();
}
