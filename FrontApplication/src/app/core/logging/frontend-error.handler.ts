import { ErrorHandler, Injectable } from '@angular/core';

import { FrontendLogMessages } from './frontend-log-messages';
import { FrontendLoggerService } from './frontend-logger.service';

@Injectable()
export class FrontendErrorHandler implements ErrorHandler {
  constructor(private readonly logger: FrontendLoggerService) {}

  handleError(error: unknown): void {
    this.logger.error(FrontendLogMessages.angularUnhandledError, error);
  }
}
