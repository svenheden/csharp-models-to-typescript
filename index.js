#!/usr/bin/env node

const fs = require('fs');
const process = require('process');
const path = require('path');
const { spawn } = require('child_process');

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

const output = config.output || 'types.d.ts';

const converter = createConverter({
    customTypeTranslations: config.customTypeTranslations || {},
    namespace: config.namespace,
    includeComments: config.includeComments ?? true,
    camelCase: config.camelCase || false,
    camelCaseOptions: config.camelCaseOptions || {},
    camelCaseEnums: config.camelCaseEnums || false,
    numericEnums: config.numericEnums || false,
    validateEmitDefaultValue: config.validateEmitDefaultValue || false,
    omitFilePathComment: config.omitFilePathComment || false,
    omitSemicolon: config.omitSemicolon || false,
    stringLiteralTypesInsteadOfEnums: config.stringLiteralTypesInsteadOfEnums || false
});

let timer = process.hrtime();

const dotnetProject = path.join(__dirname, 'lib/csharp-models-to-json');
const dotnetProcess = spawn('dotnet', ['run', `--project "${dotnetProject}"`, `"${path.resolve(configPath)}"`], { shell: true });

let stdout = '';

dotnetProcess.stdout.on('data', data => {
    stdout += data;
});

dotnetProcess.stderr.on('data', err => {
    console.error(err.toString());
});

dotnetProcess.stdout.on('end', () => {
    let json;

    //console.log(stdout);

    try {
        // Extract the JSON content between the markers
        const startMarker = '<<<<<<START_JSON>>>>>>';
        const endMarker = '<<<<<<END_JSON>>>>>>';
        const startIndex = stdout.indexOf(startMarker);
        const endIndex = stdout.indexOf(endMarker);

        if (startIndex !== -1 && endIndex !== -1 && endIndex > startIndex) {
            const jsonString = stdout.substring(startIndex + startMarker.length, endIndex).trim();
            json = JSON.parse(jsonString);
        } else {
            throw new Error('JSON markers not found or invalid order of markers.');
        }
    } catch (error) {
        return console.error([
            'The output from `csharp-models-to-json` contains invalid JSON.',
            error.message,
            stdout
        ].join('\n\n'));
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
