import http from 'k6/http';
import { apiClientHeaders, benchmarkOptions, recordHttpResponse, requireEnvironment } from './lib/benchmark.js';

export const options = benchmarkOptions();

const baseUrl = (__ENV.API_BASE_URL || 'https://localhost:7025').replace(/\/$/, '');
const path = __ENV.API_PATH || '/accounts/me';
const headers = apiClientHeaders('API', requireEnvironment('API_ACCESS_TOKEN'));

export default function () {
    const response = http.get(baseUrl + path, { headers, tags: { endpoint: path } });
    recordHttpResponse(response, [200]);
}
