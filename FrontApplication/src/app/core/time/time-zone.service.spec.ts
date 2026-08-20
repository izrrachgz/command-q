import { TestBed } from '@angular/core/testing';

import { TimeZoneService } from './time-zone.service';

describe('TimeZoneService', () => {
  it('uses the browser timezone and exposes searchable IANA values', () => {
    const service = TestBed.inject(TimeZoneService);

    expect(service.timeZones).toContain(service.browserTimeZone);
    expect(service.isValid('America/Hermosillo')).toBe(true);
    expect(service.isValid('Not/A_TimeZone')).toBe(false);
  });
});
