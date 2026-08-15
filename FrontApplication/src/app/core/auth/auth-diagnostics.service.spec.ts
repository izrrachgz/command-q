import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';

import { AuthDiagnosticsService } from './auth-diagnostics.service';
import { combineUrl } from '../config/url-builder';
import { environment } from '../../../environments/environment';

describe('AuthDiagnosticsService', () => {
  let service: AuthDiagnosticsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AuthDiagnosticsService);
  });

  it('describes identity service network failures with the failing url', () => {
    const failingUrl = combineUrl(environment.identity.authority, '.well-known/openid-configuration');
    const diagnostic = service.describe(
      new HttpErrorResponse({
        status: 0,
        url: failingUrl,
      }),
    );

    expect(diagnostic.status).toBe(0);
    expect(diagnostic.url).toBe(failingUrl);
    expect(diagnostic.message).toContain('Unable to reach');
  });
});
