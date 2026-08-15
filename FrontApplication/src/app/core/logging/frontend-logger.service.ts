import { Injectable } from '@angular/core';

import { environment } from '../../../environments/environment';
import { ApiHttpHeaders, ApiHttpMethods, ApiMimeTypes } from '../api/api-constants';
import { FrontendLogMessages } from './frontend-log-messages';

export type FrontendLogLevel = 'Debug' | 'Information' | 'Warning' | 'Error';

interface FrontendLogEntry {
  readonly timestamp: string;
  readonly level: FrontendLogLevel;
  readonly message: string;
  readonly url: string;
  readonly userAgent: string;
  readonly detail?: unknown;
}

const REDACTED = '[redacted]';
const SENSITIVE_KEY_PATTERN = /(authorization|password|secret|token|refresh|credential|clientid|clientsecret)/i;
const SENSITIVE_TEXT_ASSIGNMENT_PATTERN =
  /(["']?(?:access[_-]?token|id[_-]?token|refresh[_-]?token|authorization|password|secret|credential|client[_-]?id|client[_-]?secret|token)["']?\s*[:=]\s*["'])([^"',}\s]+)/gi;
const SENSITIVE_QUOTED_TEXT_PATTERN =
  /(\b(?:accessToken|idToken|refreshToken|clientSecret)\b\s*["'])([^"']+)(["'])/g;
const SENSITIVE_QUERY_PATTERN = /([?&](?:code|access_token|id_token|refresh_token|token)=)[^&#\s"']+/gi;
const BEARER_TOKEN_PATTERN = /(Bearer\s+)[A-Za-z0-9._~+/-]+=*/gi;
const JWT_LIKE_PATTERN = /\beyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+(?:\.[A-Za-z0-9_-]+)?\b/g;

@Injectable({ providedIn: 'root' })
export class FrontendLoggerService {
  private initialized = false;

  initialize(): void {
    if (this.initialized || !this.fileLoggingEnabled) {
      return;
    }

    this.initialized = true;
    globalThis.addEventListener?.('error', (event) => {
      this.error(FrontendLogMessages.unhandledBrowserError, {
        message: event.message,
        filename: event.filename,
        line: event.lineno,
        column: event.colno,
        error: event.error,
      });
    });
    globalThis.addEventListener?.('unhandledrejection', (event) => {
      this.error(FrontendLogMessages.unhandledPromiseRejection, event.reason);
    });
    this.information(FrontendLogMessages.frontendLoggingInitialized, {
      environment: environment.name,
      endpoint: environment.logging.file.endpoint,
    });
  }

  debug(message: string, detail?: unknown): void {
    this.log('Debug', message, detail);
  }

  information(message: string, detail?: unknown): void {
    this.log('Information', message, detail);
  }

  warning(message: string, detail?: unknown): void {
    this.log('Warning', message, detail);
  }

  error(message: string, detail?: unknown): void {
    this.log('Error', message, detail);
  }

  log(level: FrontendLogLevel, message: string, detail?: unknown): void {
    if (!this.shouldWriteConsole && !this.fileLoggingEnabled) {
      return;
    }

    const entry = this.createEntry(level, message, detail);

    if (this.shouldWriteConsole) {
      this.writeConsole(entry);
    }

    if (!this.fileLoggingEnabled) {
      return;
    }

    void fetch(environment.logging.file.endpoint, {
      method: ApiHttpMethods.post,
      headers: {
        [ApiHttpHeaders.contentType]: ApiMimeTypes.json,
      },
      body: JSON.stringify(entry),
      keepalive: true,
    }).catch(() => undefined);
  }

  private get fileLoggingEnabled(): boolean {
    return environment.name === 'development' &&
      !environment.production &&
      environment.logging.file.enabled &&
      environment.logging.file.endpoint.length > 0;
  }

  private get shouldWriteConsole(): boolean {
    return !environment.production;
  }

  private createEntry(level: FrontendLogLevel, message: string, detail?: unknown): FrontendLogEntry {
    return {
      timestamp: new Date().toISOString(),
      level,
      message: redactSensitiveText(message),
      url: redactSensitiveText(globalThis.location?.href ?? ''),
      userAgent: globalThis.navigator?.userAgent ?? '',
      detail: detail === undefined ? undefined : normalizeDetail(detail),
    };
  }

  private writeConsole(entry: FrontendLogEntry): void {
    const args = entry.detail === undefined ? [entry.message] : [entry.message, entry.detail];

    switch (entry.level) {
      case 'Debug':
        console.debug(...args);
        break;
      case 'Information':
        console.info(...args);
        break;
      case 'Warning':
        console.warn(...args);
        break;
      case 'Error':
        console.error(...args);
        break;
    }
  }
}

const normalizeDetail = (value: unknown, seen = new WeakSet<object>()): unknown => {
  if (value instanceof Error) {
    return {
      name: value.name,
      message: value.message,
      stack: value.stack,
    };
  }

  if (typeof value === 'string') {
    return redactSensitiveText(value);
  }

  if (value === null || value === undefined || typeof value !== 'object') {
    return value;
  }

  if (seen.has(value)) {
    return '[circular]';
  }

  seen.add(value);

  if (Array.isArray(value)) {
    return value.map((item) => normalizeDetail(item, seen));
  }

  return Object.fromEntries(
    Object.entries(value as Record<string, unknown>).map(([key, entryValue]) => [
      key,
      SENSITIVE_KEY_PATTERN.test(key) ? REDACTED : normalizeDetail(entryValue, seen),
    ]),
  );
};

const redactSensitiveText = (value: string): string => {
  return value
    .replace(SENSITIVE_TEXT_ASSIGNMENT_PATTERN, `$1${REDACTED}`)
    .replace(SENSITIVE_QUOTED_TEXT_PATTERN, `$1${REDACTED}$3`)
    .replace(SENSITIVE_QUERY_PATTERN, `$1${REDACTED}`)
    .replace(BEARER_TOKEN_PATTERN, `$1${REDACTED}`)
    .replace(JWT_LIKE_PATTERN, REDACTED);
};
