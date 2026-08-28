import { TextConfirmationComponent } from './text-confirmation.component';

describe('TextConfirmationComponent', () => {
  it('accepts matching text without case sensitivity by default', () => {
    const component = new TextConfirmationComponent();
    component.expectedText = 'DELETE';
    component.value = 'delete';

    expect(component.matchesExpectedText).toBe(true);
  });

  it('honors case-sensitive confirmation when enabled', () => {
    const component = new TextConfirmationComponent();
    component.expectedText = 'DELETE';
    component.value = 'delete';
    component.caseSensitive = true;

    expect(component.matchesExpectedText).toBe(false);
  });

  it('passes the confirmed value to the configured action', () => {
    const component = new TextConfirmationComponent();
    const action = vi.fn();
    component.confirmAction = action;
    component.value = 'command-q';

    component.runConfirmAction();

    expect(action).toHaveBeenCalledWith('command-q');
  });
});
