import exec from 'k6/execution';
import http from 'k6/http';
import {
    acquireInternalApiToken,
    apiClientHeaders,
    benchmarkOptions,
    recordHttpResponse
} from './lib/benchmark.js';

export const options = benchmarkOptions();

const baseUrl = (__ENV.EMAIL_API_BASE_URL || 'https://localhost:7325').replace(/\/$/, '');
const recipient = __ENV.BENCHMARK_EMAIL || 'benchmark@example.test';

export function setup() {
    return { accessToken: acquireInternalApiToken(baseUrl, 'EMAIL_API', http) };
}

export default function (data) {
    const correlation = exec.vu.idInTest + '-' + exec.scenario.iterationInTest + '-' + Date.now();
    const response = http.post(
        baseUrl + '/v1/email-queue',
        JSON.stringify({
            message: {
                to: [{ address: recipient, name: 'Benchmark recipient' }],
                subject: 'command-q queue benchmark',
                textBody: 'Benchmark message ' + correlation
            },
            correlationId: correlation
        }),
        {
            headers: apiClientHeaders('EMAIL_API', data.accessToken),
            tags: { endpoint: '/v1/email-queue', transport: 'rest' }
        }
    );
    recordHttpResponse(response, [202]);
}
