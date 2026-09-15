import { Counter, Rate } from 'k6/metrics';

export const businessSuccess = new Rate('business_success');
export const serverErrors = new Rate('server_errors');
export const throttledRequests = new Counter('throttled_requests');
export const unexpectedResponses = new Rate('unexpected_responses');

function integerEnvironment(name, fallback) {
    const value = __ENV[name];
    if (!value) {
        return fallback;
    }

    const parsed = Number.parseInt(value, 10);
    if (!Number.isInteger(parsed) || parsed <= 0) {
        throw new Error(name + ' must be a positive integer.');
    }

    return parsed;
}

export function requireEnvironment(name) {
    const value = __ENV[name];
    if (!value || !value.trim()) {
        throw new Error(name + ' is required.');
    }

    return value.trim();
}

export function benchmarkOptions(protocol = 'http') {
    const mode = (__ENV.MODE || 'smoke').toLowerCase();
    const target = integerEnvironment('TARGET_VUS', 20);
    const profiles = {
        smoke: [{ duration: __ENV.SMOKE_DURATION || '30s', target: 1 }],
        ramp: [
            { duration: __ENV.RAMP_UP_DURATION || '30s', target },
            { duration: __ENV.HOLD_DURATION || '2m', target },
            { duration: __ENV.RAMP_DOWN_DURATION || '30s', target: 0 }
        ],
        spike: [
            { duration: __ENV.SPIKE_UP_DURATION || '10s', target },
            { duration: __ENV.SPIKE_HOLD_DURATION || '30s', target },
            { duration: __ENV.SPIKE_DOWN_DURATION || '20s', target: 0 }
        ],
        soak: [
            { duration: __ENV.RAMP_UP_DURATION || '1m', target },
            { duration: __ENV.SOAK_DURATION || '30m', target },
            { duration: __ENV.RAMP_DOWN_DURATION || '1m', target: 0 }
        ]
    };

    const stages = profiles[mode];
    if (!stages) {
        throw new Error('MODE must be one of: ' + Object.keys(profiles).join(', ') + '.');
    }

    const durationMetric = protocol === 'grpc' ? 'grpc_req_duration' : 'http_req_duration';
    return {
        discardResponseBodies: true,
        insecureSkipTLSVerify: (__ENV.INSECURE_SKIP_TLS_VERIFY || 'true').toLowerCase() === 'true',
        summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'],
        scenarios: {
            benchmark: {
                executor: 'ramping-vus',
                gracefulRampDown: '10s',
                gracefulStop: '30s',
                stages
            }
        },
        thresholds: {
            [durationMetric]: [
                'p(95)<' + integerEnvironment('P95_MS', 500),
                'p(99)<' + integerEnvironment('P99_MS', 1000)
            ],
            server_errors: ['rate<' + (__ENV.MAX_SERVER_ERROR_RATE || '0.001')],
            unexpected_responses: ['rate<' + (__ENV.MAX_UNEXPECTED_RATE || '0.01')]
        }
    };
}

export function recordHttpResponse(response, expectedStatuses) {
    const succeeded = expectedStatuses.includes(response.status);
    const throttled = response.status === 429;
    businessSuccess.add(succeeded);
    serverErrors.add(response.status >= 500);
    throttledRequests.add(throttled ? 1 : 0);
    unexpectedResponses.add(!succeeded && !throttled);
}

export function apiClientHeaders(prefix, bearerToken) {
    const headerName = requireEnvironment(prefix + '_CLIENT_HEADER');
    const headerValue = requireEnvironment(prefix + '_CLIENT_KEY');
    const headers = {
        [headerName]: headerValue,
        'Content-Type': 'application/json'
    };
    if (bearerToken) {
        headers.Authorization = 'Bearer ' + bearerToken;
    }

    return headers;
}

export function acquireInternalApiToken(baseUrl, prefix, http) {
    const response = http.post(
        baseUrl + '/auth/token',
        JSON.stringify({
            userName: requireEnvironment(prefix + '_USERNAME'),
            password: requireEnvironment(prefix + '_PASSWORD')
        }),
        { headers: apiClientHeaders(prefix), responseType: 'text' }
    );

    if (response.status !== 200) {
        throw new Error(prefix + ' token request failed with HTTP ' + response.status + '.');
    }

    const accessToken = response.json('accessToken');
    if (!accessToken) {
        throw new Error(prefix + ' token response did not contain accessToken.');
    }

    return accessToken;
}
