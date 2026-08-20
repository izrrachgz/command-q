import {
  accountLocalDateTimeToUtcIso,
  accountLocalDateToUtcIso,
  ensureUtcSuffix,
  toUtcIso,
} from './date-time-utc';

describe('UTC date/time helpers', () => {
  it('serializes values with an explicit UTC suffix', () => {
    expect(toUtcIso('2026-08-17T18:00:00Z')).toBe('2026-08-17T18:00:00.000Z');
    expect(ensureUtcSuffix('2026-08-17T18:00:00')).toBe('2026-08-17T18:00:00Z');
  });

  it('converts an account-local wall clock value to UTC', () => {
    expect(accountLocalDateTimeToUtcIso('2026-08-17T11:00', 'America/Hermosillo')).toBe(
      '2026-08-17T18:00:00.000Z',
    );
  });

  it('converts account-local date boundaries to UTC ISO strings', () => {
    expect(accountLocalDateToUtcIso('2026-08-17', 'America/Hermosillo')).toBe(
      '2026-08-17T07:00:00.000Z',
    );
    expect(accountLocalDateToUtcIso('2026-08-17', 'America/Hermosillo', true)).toBe(
      '2026-08-18T06:59:59.000Z',
    );
  });
});
