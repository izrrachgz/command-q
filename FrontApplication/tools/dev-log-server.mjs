import { createServer } from 'node:http';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';
import { mkdir, readdir, stat, unlink, appendFile } from 'node:fs/promises';

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = join(__dirname, '..');
const logDirectory = process.env.APP_FRONT_LOG_DIRECTORY ?? join(root, 'Logs');
const port = Number.parseInt(process.env.APP_FRONT_LOG_PORT ?? '4301', 10);
const host = process.env.APP_FRONT_LOG_HOST ?? '127.0.0.1';
const maxBodyBytes = 64 * 1024;
const maxFileBytes = 10 * 1024 * 1024;
const retainedFileCount = 16;
const logPath = '/front-dev-log';
const allowedOrigins = new Set(createLocalOrigins());
const levelMap = new Map([
  ['Debug', 'DBG'],
  ['Information', 'INF'],
  ['Warning', 'WRN'],
  ['Error', 'ERR'],
]);

const server = createServer(async (request, response) => {
  try {
    if (request.url !== logPath) {
      response.writeHead(404);
      response.end();
      return;
    }

    const origin = request.headers.origin;
    if (origin && allowedOrigins.has(origin)) {
      response.setHeader('Access-Control-Allow-Origin', origin);
      response.setHeader('Vary', 'Origin');
      response.setHeader('Access-Control-Allow-Headers', 'content-type');
      response.setHeader('Access-Control-Allow-Methods', 'POST, OPTIONS');
    }

    if (request.method === 'OPTIONS') {
      response.writeHead(204);
      response.end();
      return;
    }

    if (request.method !== 'POST') {
      response.writeHead(405);
      response.end();
      return;
    }

    if (origin && !allowedOrigins.has(origin)) {
      response.writeHead(403);
      response.end();
      return;
    }

    const body = (await readRequestBody(request)).replace(/^\uFEFF/, '');
    await writeLogEntry(JSON.parse(body));
    response.writeHead(204);
    response.end();
  } catch (error) {
    console.error('[front-dev-log] failed to write log entry', error);
    response.writeHead(400);
    response.end();
  }
});

server.listen(port, host, () => {
  console.info(`[front-dev-log] writing development logs to ${logDirectory}`);
  console.info(`[front-dev-log] listening on http://${host}:${port}${logPath}`);
});

const shutdown = () => {
  server.close(() => process.exit(0));
};

process.on('SIGINT', shutdown);
process.on('SIGTERM', shutdown);

function createLocalOrigins() {
  const origins = [];

  for (let portNumber = 4200; portNumber <= 4205; portNumber += 1) {
    origins.push(`http://localhost:${portNumber}`);
    origins.push(`http://127.0.0.1:${portNumber}`);
  }

  return origins;
}

async function readRequestBody(request) {
  const chunks = [];
  let size = 0;

  for await (const chunk of request) {
    size += chunk.length;

    if (size > maxBodyBytes) {
      throw new Error('Log entry payload is too large.');
    }

    chunks.push(chunk);
  }

  return Buffer.concat(chunks).toString('utf8');
}

async function writeLogEntry(entry) {
  await mkdir(logDirectory, { recursive: true });
  const timestamp = parseTimestamp(entry.timestamp);
  const filePath = await getLogFilePath(timestamp);
  const line = formatLogLine(entry, timestamp);

  await appendFile(filePath, line, 'utf8');
  await pruneOldLogs();
}

function parseTimestamp(value) {
  const parsed = new Date(typeof value === 'string' ? value : Date.now());

  return Number.isNaN(parsed.getTime()) ? new Date() : parsed;
}

async function getLogFilePath(timestamp) {
  const datePart = formatDatePart(timestamp);

  for (let index = 0; index < 100; index += 1) {
    const suffix = index === 0 ? '' : `-${index}`;
    const filePath = join(logDirectory, `front-application-log-${datePart}${suffix}.txt`);

    try {
      const info = await stat(filePath);

      if (info.size < maxFileBytes) {
        return filePath;
      }
    } catch {
      return filePath;
    }
  }

  return join(logDirectory, `front-application-log-${datePart}-overflow.txt`);
}

function formatLogLine(entry, timestamp) {
  const level = levelMap.get(entry.level) ?? 'INF';
  const message = sanitizeLine(typeof entry.message === 'string' ? entry.message : 'Frontend log entry.');
  const metadata = JSON.stringify({
    url: entry.url,
    userAgent: entry.userAgent,
    detail: entry.detail,
  });

  return `${formatTimestamp(timestamp)} [${level}] ${message} ${metadata}\n`;
}

function formatTimestamp(timestamp) {
  const offsetMinutes = -timestamp.getTimezoneOffset();
  const offsetSign = offsetMinutes >= 0 ? '+' : '-';
  const offsetHours = Math.floor(Math.abs(offsetMinutes) / 60);
  const offsetRemainder = Math.abs(offsetMinutes) % 60;

  return `${timestamp.getFullYear()}-${pad(timestamp.getMonth() + 1)}-${pad(timestamp.getDate())} ` +
    `${pad(timestamp.getHours())}:${pad(timestamp.getMinutes())}:${pad(timestamp.getSeconds())}.` +
    `${pad(timestamp.getMilliseconds(), 3)} ${offsetSign}${pad(offsetHours)}:${pad(offsetRemainder)}`;
}

function formatDatePart(timestamp) {
  return `${timestamp.getFullYear()}${pad(timestamp.getMonth() + 1)}${pad(timestamp.getDate())}`;
}

function sanitizeLine(value) {
  return value.replace(/[\r\n]+/g, ' ').trim();
}

function pad(value, size = 2) {
  return String(value).padStart(size, '0');
}

async function pruneOldLogs() {
  const files = (await readdir(logDirectory))
    .filter((file) => /^front-application-log-\d{8}(?:-\d+|-overflow)?\.txt$/.test(file))
    .sort()
    .reverse();

  await Promise.all(files.slice(retainedFileCount).map((file) => unlink(join(logDirectory, file))));
}
