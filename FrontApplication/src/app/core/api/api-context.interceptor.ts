import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { environment } from '../../../environments/environment';
import { I18nService } from '../i18n/i18n.service';
import { ApiHttpHeaders } from './api-constants';

export const apiContextInterceptor: HttpInterceptorFn = (request, next) => {
  if (!request.url.startsWith(environment.api.baseUrl)) {
    return next(request);
  }

  const i18n = inject(I18nService);
  const headers: Record<string, string> = {
    [ApiHttpHeaders.acceptLanguage]: i18n.currentCulture,
    [ApiHttpHeaders.correlationId]: createCorrelationId(),
    [environment.api.clientHeaderName]: environment.api.clientId,
  };

  return next(request.clone({ setHeaders: headers }));
};

const createCorrelationId = (): string => {
  if (globalThis.crypto?.randomUUID) {
    return globalThis.crypto.randomUUID();
  }

  return `${Date.now()}-${Math.random().toString(16).slice(2)}`;
};
