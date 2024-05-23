const fs = require('fs');

const configArg = process.argv.find(x => x.startsWith('--config='));

if (!configArg) {
    throw Error('No configuration file for `csharp-models-to-typescript` provided.');
}

const configPath = configArg.substr('--config='.length);
let config;

try {
    unparsedConfig = fs.readFileSync(configPath, 'utf8');
} catch (error) {
    throw Error(`Configuration file "${configPath}" not found.\n${error.message}`);
}

try {
    config = JSON.parse(unparsedConfig);
} catch (error) {
    throw Error(`Configuration file "${configPath}" contains invalid JSON.\n${error.message}`);
}

const output = config.output || 'types.d.ts';

if (!fs.existsSync(output))
    throw Error(`Can't find output file: ${output}`)

const file = fs.readFileSync(output, 'utf8')
if (file.length === 0)
  throw Error(`File '${output}' is empty`)