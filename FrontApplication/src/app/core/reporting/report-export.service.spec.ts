import { TestBed } from '@angular/core/testing';

import { ReportExportService } from './report-export.service';

describe('ReportExportService', () => {
  it('creates the service', () => {
    TestBed.configureTestingModule({});

    expect(TestBed.inject(ReportExportService)).toBeTruthy();
  });
});
