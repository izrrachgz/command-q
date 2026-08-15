import { FrontendLoggerService } from './frontend-logger.service';
import { FrontendLogMessages } from './frontend-log-messages';

describe('FrontendLoggerService', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(null, { status: 204 })));
    vi.spyOn(console, 'error').mockImplementation(() => undefined);
  });

  afterEach(() => {
    vi.restoreAllMocks();
    vi.unstubAllGlobals();
  });

  it('writes development log entries to the local log endpoint with sensitive values redacted', async () => {
    const logger = new FrontendLoggerService();

    logger.error('Authentication failed.', {
      accessToken: 'secret-token',
      nested: {
        password: 'secret-password',
        reason: 'network',
      },
    });
    await Promise.resolve();

    expect(fetch).toHaveBeenCalledTimes(1);
    const [url, init] = vi.mocked(fetch).mock.calls[0];
    const body = JSON.parse(String(init?.body));

    expect(url).toBe('/front-dev-log');
    expect(body.level).toBe('Error');
    expect(body.message).toBe('Authentication failed.');
    expect(body.detail.accessToken).toBe('[redacted]');
    expect(body.detail.nested.password).toBe('[redacted]');
    expect(body.detail.nested.reason).toBe('network');
  });

  it('redacts sensitive tokens embedded in text values', async () => {
    const logger = new FrontendLoggerService();
    const token = 'eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiIxMjMifQ.signature';

    logger.debug(FrontendLogMessages.oidcClientDebug, {
      message: `AuthResult { "access_token": "${token}", "refresh_token": "${token}" } storing the accessToken '${token}' using Bearer ${token} from /auth/callback?code=abc123`,
    });
    await Promise.resolve();

    const [, init] = vi.mocked(fetch).mock.calls[0];
    const body = JSON.parse(String(init?.body));
    const message = body.detail.message as string;

    expect(message).not.toContain(token);
    expect(message).toContain('"access_token": "[redacted]');
    expect(message).toContain('"refresh_token": "[redacted]');
    expect(message).toContain("accessToken '[redacted]'");
    expect(message).toContain('Bearer [redacted]');
    expect(message).toContain('code=[redacted]');
  });
});
