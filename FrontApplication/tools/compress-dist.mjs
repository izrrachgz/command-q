import { brotliCompress, gzip } from 'node:zlib';
import { promisify } from 'node:util';
import { readdir, readFile, stat, writeFile } from 'node:fs/promises';
import { join } from 'node:path';

const gzipAsync = promisify(gzip);
const brotliAsync = promisify(brotliCompress);
const target = process.argv[2] ?? 'dist/FrontApplication/browser';
const compressibleExtensions = new Set(['.css', '.html', '.js', '.json', '.svg', '.txt', '.xml']);

const files = await collectFiles(target);

await Promise.all(
  files
    .filter((file) => compressibleExtensions.has(extension(file)))
    .flatMap(async (file) => {
      const content = await readFile(file);
      const [gzipped, brotlied] = await Promise.all([gzipAsync(content, { level: 9 }), brotliAsync(content)]);

      await Promise.all([writeFile(`${file}.gz`, gzipped), writeFile(`${file}.br`, brotlied)]);
    }),
);

console.log(`Compressed ${files.length} files under ${target}`);

async function collectFiles(directory) {
  const entries = await readdir(directory, { withFileTypes: true });
  const children = await Promise.all(
    entries.map(async (entry) => {
      const path = join(directory, entry.name);
      return entry.isDirectory() ? collectFiles(path) : path;
    }),
  );

  return children.flat();
}

function extension(file) {
  const index = file.lastIndexOf('.');
  return index === -1 ? '' : file.slice(index).toLowerCase();
}

await stat(target);
