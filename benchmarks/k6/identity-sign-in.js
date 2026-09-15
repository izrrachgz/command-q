import http from 'k6/http';
import { benchmarkOptions, recordHttpResponse, requireEnvironment } from './lib/benchmark.js';

export const options = benchmarkOptions();

const baseUrl = (__ENV.IDENTITY_BASE_URL || 'https://localhost:7125').replace(/\/$/, '');
const body = JSON.stringify({
    usernameOrEmail: requireEnvironment('IDENTITY_USERNAME'),
    password: requireEnvironment('IDENTITY_PASSWORD'),
    rememberMe: false
});
const requestOptions = {
    headers: { 'Content-Type': 'application/json' },
    tags: { endpoint: '/account/sign-in' }
};

export default function () {
    const response = http.post(baseUrl + '/account/sign-in', body, requestOptions);
    recordHttpResponse(response, [204]);
}
