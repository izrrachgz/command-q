import { AbstractControl } from '@angular/forms';

import { DateTimeRangeInputComponent, DateTimeRangeValue } from './date-time-range-input.component';

describe('DateTimeRangeInputComponent', () => {
  it('requires both values when required', () => {
    const component = createComponent();
    component.required = true;

    expect(component.validate(controlWith(null))).toEqual({ required: true });
  });

  it('rejects incomplete ranges', () => {
    const component = createComponent();

    expect(component.validate(controlWith({ start: '2026-08-15T09:00', end: '' }))).toEqual({
      dateTimeRangeIncomplete: true,
    });
  });

  it('rejects ranges where the start is after the end', () => {
    const component = createComponent();

    expect(component.validate(controlWith({ start: '2026-08-15T18:00', end: '2026-08-15T09:00' }))).toEqual({
      dateTimeRangeOrder: true,
    });
  });

  it('validates min and max bounds', () => {
    const component = createComponent();
    component.min = '2026-08-15T08:00';
    component.max = '2026-08-15T18:00';

    expect(component.validate(controlWith({ start: '2026-08-15T07:59', end: '2026-08-15T09:00' }))).toEqual({
      min: { value: component.min },
    });
    expect(component.validate(controlWith({ start: '2026-08-15T09:00', end: '2026-08-15T18:01' }))).toEqual({
      max: { value: component.max },
    });
  });

  it('accepts a complete ordered range inside optional bounds', () => {
    const component = createComponent();
    component.min = '2026-08-15T08:00';
    component.max = '2026-08-15T18:00';

    expect(component.validate(controlWith({ start: '2026-08-15T09:00', end: '2026-08-15T17:00' }))).toBeNull();
  });

  it('emits null when the range is cleared', () => {
    const component = createComponent();
    const emitted: Array<DateTimeRangeValue | null> = [];
    component.registerOnChange((value) => emitted.push(value));

    component.writeValue({ start: '2026-08-15T09:00', end: '2026-08-15T17:00' });
    component.handleInput('start', inputEvent(''));
    component.handleInput('end', inputEvent(''));

    expect(emitted[emitted.length - 1]).toBeNull();
  });
});

function createComponent(): DateTimeRangeInputComponent {
  return new DateTimeRangeInputComponent(null);
}

function controlWith(value: DateTimeRangeValue | null): AbstractControl {
  return { value } as AbstractControl;
}

function inputEvent(value: string): Event {
  return { target: { value } } as unknown as Event;
}
