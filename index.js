#!/usr/bin/env node

const fs = require('fs');
const process = require('process');
const path = require('path');
const { exec } = require('child_process');

const createConverter = require('./converter');

const configArg = process.argv.find(x => x.startsWith('--config='));

if (!configArg) {
    return console.error('No configuration file for `csharp-models-to-typescript` provided.');
}

const configPath = configArg.substr('--config='.length);
let config;

try {
    unparsedConfig = fs.readFileSync(configPath, 'utf8');
} catch (error) {
    return console.error(`Configuration file "${configPath}" not found.`);
}

try {
    config = JSON.parse(unparsedConfig);
} catch (error) {
    return console.error(`Configuration file "${configPath}" contains invalid JSON.`);
}

const output = config.output || 'types.json';

const converter = createConverter({
    customTypeTranslations: config.customTypeTranslations || {},
    namespace: config.namespace,
    camelCase: config.camelCase || false,
    stringLiteralTypesInsteadOfEnums: config.stringLiteralTypesInsteadOfEnums || false
});

const dotnetProject = path.join(__dirname, 'lib/csharp-models-to-json');

let timer = process.hrtime();

exec(`dotnet run --project "${dotnetProject}" "${path.resolve(configPath)}"`, (err, stdout) => {
    if (err) {
        return console.error(err);
    }

    let json;

    try {
        json = JSON.parse(stdout);
    } catch (error) {
        return console.error('The output from `csharp-models-to-json` contains invalid JSON.');
    }

    const types = converter(json);

    fs.writeFile(output, types, err => {
        if (err) {
            return console.error(err);
        }

        timer = process.hrtime(timer);
        console.log('Done in %d.%d seconds.', timer[0], timer[1]);
    });
});
