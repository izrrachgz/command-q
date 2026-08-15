import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, finalize, map, Observable, of, shareReplay, switchMap, tap, throwError, timeout } from 'rxjs';

import { environment } from '../../../environments/environment';
import { combineUrl } from '../config/url-builder';
import { FrontendLogMessages } from '../logging/frontend-log-messages';
import { FrontendLoggerService } from '../logging/frontend-logger.service';
import { AccountModel } from '../models/account.model';
import { BasicResponse } from '../models/api-response';
import { AuthFacade } from './auth.facade';

const PROVISION_TIMEOUT_MS = 15000;

@Injectable({ providedIn: 'root' })
export class AccountProvisionService {
  private ensureRequest$: Observable<AccountModel | null> | null = null;
  private loadRequest$: Observable<AccountModel | null> | null = null;

  constructor(
    private readonly http: HttpClient,
    private readonly auth: AuthFacade,
    private readonly logger: FrontendLoggerService,
  ) {}

  loadProvisionedAccount(): Observable<AccountModel | null> {
    if (this.auth.snapshot.account) {
      return of(this.auth.snapshot.account);
    }

    if (!this.loadRequest$) {
      this.loadRequest$ = this.auth.loadApplicationAccount().pipe(
        timeout({ first: PROVISION_TIMEOUT_MS }),
        map((account) => account ?? null),
        tap((account) => {
          if (account) {
            this.auth.setApplicationAccount(account);
          }
        }),
        catchError((error) => {
          this.logger.debug(FrontendLogMessages.provisionAccountLoadSkipped, error);
          return of(null);
        }),
        finalize(() => (this.loadRequest$ = null)),
        shareReplay({ bufferSize: 1, refCount: false }),
      );
    }

    return this.loadRequest$;
  }

  ensureProvisioned(): Observable<AccountModel | null> {
    if (this.auth.snapshot.account) {
      return of(this.auth.snapshot.account);
    }

    if (!this.ensureRequest$) {
      this.ensureRequest$ = this.loadProvisionedAccount().pipe(
        switchMap((account) => {
          if (account) {
            return of(account);
          }

          return this.createAndLoad();
        }),
        tap((account) => this.auth.setApplicationAccount(account)),
        finalize(() => (this.ensureRequest$ = null)),
        shareReplay({ bufferSize: 1, refCount: false }),
      );
    }

    return this.ensureRequest$;
  }

  createAndLoad(): Observable<AccountModel | null> {
    this.logger.information(FrontendLogMessages.provisionStarted);

    return this.http
      .post<BasicResponse>(combineUrl(environment.api.baseUrl, environment.api.endpoints.account.create), {})
      .pipe(
        timeout({ first: PROVISION_TIMEOUT_MS }),
        switchMap(() => this.auth.loadApplicationAccount()),
        timeout({ first: PROVISION_TIMEOUT_MS }),
        map((account) => account ?? null),
        tap((account) => {
          if (account) {
            this.logger.information(FrontendLogMessages.provisionCompleted, { accountId: account.id });
            return;
          }

          this.logger.warning(FrontendLogMessages.provisionCompletedWithoutPayload);
        }),
        catchError((error) => {
          this.logger.error(FrontendLogMessages.provisionFailed, error);
          return throwError(() => error);
        }),
      );
  }

  provisionOrThrow(): Observable<AccountModel | null> {
    return this.ensureProvisioned().pipe(
      switchMap((account) => (
        account ? of(account) : throwError(() => new Error(FrontendLogMessages.accountProfileMissing))
      )),
    );
  }
}
