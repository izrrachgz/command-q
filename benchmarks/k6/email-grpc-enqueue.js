import exec from 'k6/execution';
import grpc from 'k6/net/grpc';
import http from 'k6/http';
import {
    acquireInternalApiToken,
    apiClientHeaders,
    benchmarkOptions,
    businessSuccess,
    serverErrors,
    throttledRequests,
    unexpectedResponses
} from './lib/benchmark.js';

export const options = benchmarkOptions('grpc');

const client = new grpc.Client();
client.load(['../../../command-emails/Email.Api/Protos'], 'email_queue.proto');

const restBaseUrl = (__ENV.EMAIL_API_BASE_URL || 'https://localhost:7325').replace(/\/$/, '');
const grpcAddress = __ENV.EMAIL_GRPC_ADDRESS || 'localhost:7325';
const recipient = __ENV.BENCHMARK_EMAIL || 'benchmark@example.test';
let connected = false;

export function setup() {
    return { accessToken: acquireInternalApiToken(restBaseUrl, 'EMAIL_API', http) };
}

export default function (data) {
    if (!connected) {
        client.connect(grpcAddress, {
            plaintext: (__ENV.EMAIL_GRPC_PLAINTEXT || 'false').toLowerCase() === 'true'
        });
        connected = true;
    }

    const correlation = exec.vu.idInTest + '-' + exec.scenario.iterationInTest + '-' + Date.now();
    const headers = apiClientHeaders('EMAIL_API', data.accessToken);
    delete headers['Content-Type'];
    const metadata = {};
    for (const [name, value] of Object.entries(headers)) {
        metadata[name.toLowerCase()] = value;
    }

    const response = client.invoke(
        'command.emails.v1.EmailQueue/Enqueue',
        {
            message: {
                to: [{ address: recipient, name: 'Benchmark recipient' }],
                subject: 'command-q gRPC queue benchmark',
                textBody: 'Benchmark message ' + correlation
            },
            correlationId: correlation
        },
        { metadata, tags: { endpoint: 'EmailQueue/Enqueue', transport: 'grpc' } }
    );

    const succeeded = response.status === grpc.StatusOK;
    const throttled = response.status === grpc.StatusResourceExhausted;
    businessSuccess.add(succeeded);
    serverErrors.add([
        grpc.StatusUnknown,
        grpc.StatusInternal,
        grpc.StatusUnavailable,
        grpc.StatusDataLoss
    ].includes(response.status));
    throttledRequests.add(throttled ? 1 : 0);
    unexpectedResponses.add(!succeeded && !throttled);
}
