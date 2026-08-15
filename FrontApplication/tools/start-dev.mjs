import { existsSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';
import { spawn } from 'node:child_process';

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = join(__dirname, '..');
const extraArgs = process.argv.slice(2);
const nodeExecutable = process.execPath;
const localNgScript = join(root, 'node_modules', '@angular', 'cli', 'bin', 'ng.js');
const useLocalNg = existsSync(localNgScript);
const children = [];
let shuttingDown = false;

const logWriter = startChild('front-dev-log', nodeExecutable, [join(root, 'tools', 'dev-log-server.mjs')], {}, false);
if (logWriter) {
  children.push(logWriter);
}

const angularServer = startChild(
  'angular',
  useLocalNg ? nodeExecutable : (process.platform === 'win32' ? 'npx.cmd' : 'npx'),
  useLocalNg
    ? [localNgScript, 'serve', '--configuration', 'development', ...extraArgs]
    : ['ng', 'serve', '--configuration', 'development', ...extraArgs],
  { shell: !useLocalNg && process.platform === 'win32' },
);
children.push(angularServer);

if (logWriter) {
  logWriter.on('exit', (code, signal) => {
    if (shuttingDown) {
      return;
    }

    console.warn(`[front-dev-log] stopped (${describeExit(code, signal)}); Angular will continue without local file logs.`);
  });
}

angularServer.on('exit', (code, signal) => {
  if (shuttingDown) {
    return;
  }

  shuttingDown = true;
  stopChildren(angularServer.pid);
  process.exit(code ?? (signal ? 1 : 0));
});

process.on('SIGINT', () => stopAndExit(0));
process.on('SIGTERM', () => stopAndExit(0));

function startChild(name, command, args, options = {}, required = true) {
  try {
    const child = spawn(command, args, {
      cwd: root,
      env: process.env,
      stdio: 'inherit',
      ...options,
    });

    child.on('error', (error) => {
      if (shuttingDown) {
        return;
      }

      if (!required) {
        console.warn(`[${name}] failed after launch; Angular will continue without local file logs.`, error);
        return;
      }

      console.error(`[${name}] failed to start`, error);
      shuttingDown = true;
      stopChildren(child.pid);
      process.exit(1);
    });

    return child;
  } catch (error) {
    if (required) {
      console.error(`[${name}] failed to start`, error);
      stopChildren();
      process.exit(1);
    }

    console.warn(`[${name}] failed to start; Angular will continue without local file logs.`, error);
    return null;
  }
}

function stopAndExit(code) {
  if (shuttingDown) {
    return;
  }

  shuttingDown = true;
  stopChildren();
  process.exit(code);
}

function stopChildren(exceptPid) {
  for (const child of children) {
    if (child.pid && child.pid !== exceptPid) {
      child.kill();
    }
  }
}

function describeExit(code, signal) {
  if (code !== null && code !== undefined) {
    return `code ${code}`;
  }

  return signal ? `signal ${signal}` : 'no exit code';
}
