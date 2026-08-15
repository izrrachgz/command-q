import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';

import { environment } from '../../../environments/environment';
import { FrontendLogMessages } from '../logging/frontend-log-messages';
import { FrontendLoggerService } from '../logging/frontend-logger.service';

export interface AuthDiagnostic {
  readonly message: string;
  readonly status?: number;
  readonly url?: string;
  readonly detail?: unknown;
}

@Injectable({ providedIn: 'root' })
export class AuthDiagnosticsService {
  constructor(private readonly logger: FrontendLoggerService) {}

  describe(error: unknown): AuthDiagnostic {
    if (error instanceof HttpErrorResponse) {
      return {
        message: this.describeHttpError(error),
        status: error.status,
        url: error.url ?? undefined,
        detail: error.error,
      };
    }

    if (error instanceof Error) {
      return {
        message: error.message,
        detail: error,
      };
    }

    return {
      message: this.stringify(error),
      detail: error,
    };
  }

  log(operation: string, error: unknown): void {
    if (environment.production) {
      return;
    }

    const diagnostic = this.describe(error);
    this.logger.error(FrontendLogMessages.authOperationFailed(operation, diagnostic.message), diagnostic);
  }

  private describeHttpError(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return FrontendLogMessages.identityUnreachable(error.url);
    }

    return FrontendLogMessages.identityHttpError(error.status, error.statusText, error.url);
  }

  private stringify(value: unknown): string {
    if (typeof value === 'string') {
      return value;
    }

    if (value === null || value === undefined) {
      return FrontendLogMessages.unknownAuthenticationError;
    }

    try {
      return JSON.stringify(value);
    } catch {
      return String(value);
    }
  }
}
