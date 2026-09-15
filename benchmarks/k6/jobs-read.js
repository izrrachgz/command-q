import http from 'k6/http';
import {
    acquireInternalApiToken,
    apiClientHeaders,
    benchmarkOptions,
    recordHttpResponse
} from './lib/benchmark.js';

export const options = benchmarkOptions();

const baseUrl = (__ENV.JOBS_API_BASE_URL || 'https://localhost:7225').replace(/\/$/, '');

export function setup() {
    return { accessToken: acquireInternalApiToken(baseUrl, 'JOBS_API', http) };
}

export default function (data) {
    const response = http.get(
        baseUrl + '/jobs?page=1&pageSize=25',
        {
            headers: apiClientHeaders('JOBS_API', data.accessToken),
            tags: { endpoint: '/jobs', transport: 'rest' }
        }
    );
    recordHttpResponse(response, [200]);
}
