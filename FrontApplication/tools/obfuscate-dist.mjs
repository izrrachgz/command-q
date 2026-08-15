import { readdir, readFile, writeFile } from 'node:fs/promises';
import { join } from 'node:path';
import JavaScriptObfuscator from 'javascript-obfuscator';

const target = process.argv[2] ?? 'dist/FrontApplication/browser';
const files = (await collectFiles(target)).filter((file) => file.endsWith('.js'));

await Promise.all(
  files.map(async (file) => {
    const source = await readFile(file, 'utf8');
    const result = JavaScriptObfuscator.obfuscate(source, {
      compact: true,
      controlFlowFlattening: false,
      deadCodeInjection: false,
      identifierNamesGenerator: 'hexadecimal',
      renameGlobals: false,
      rotateStringArray: true,
      selfDefending: false,
      stringArray: true,
      stringArrayThreshold: 0.25,
    });

    await writeFile(file, result.getObfuscatedCode());
  }),
);

console.log(`Obfuscated ${files.length} JavaScript files under ${target}`);

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
