import { Injectable } from '@angular/core';
import { AbstractLoggerService } from 'angular-auth-oidc-client';

import { FrontendLogMessages } from '../logging/frontend-log-messages';
import { FrontendLoggerService } from '../logging/frontend-logger.service';

@Injectable()
export class OidcConsoleLoggerService implements AbstractLoggerService {
  constructor(private readonly logger: FrontendLoggerService) {}

  logError(message: string | object, ...args: unknown[]): void {
    this.logger.error(FrontendLogMessages.oidcClientError, this.createDetail(message, args));
  }

  logWarning(message: string | object, ...args: unknown[]): void {
    this.logger.warning(FrontendLogMessages.oidcClientWarning, this.createDetail(message, args));
  }

  logDebug(message: string | object, ...args: unknown[]): void {
    this.logger.debug(FrontendLogMessages.oidcClientDebug, this.createDetail(message, args));
  }

  private createDetail(message: string | object, args: unknown[]): Record<string, unknown> {
    return {
      message,
      args,
    };
  }
}
