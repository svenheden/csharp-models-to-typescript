const glob = require('glob');
const fs = require('fs');
const { exec } = require('child_process');

const flatten = arr => arr.reduce((a, b) => a.concat(b), []);
const unique = arr => arr.filter((elem, pos, arr2) => arr2.indexOf(elem) == pos);
const diff = (arr1, arr2) => arr1.filter(i => arr2.indexOf(i) < 0);
const uniqueFilesFromGlobPatterns = patterns => unique(flatten(patterns.map(pattern => glob.sync(pattern))));

const convertJsonToTypes = json => {
    // @todo implement
    return json;
}

let config;

try {
    unparsedConfig = fs.readFileSync('./csharp-models-to-json.json', 'utf8');
    config = JSON.parse(unparsedConfig);
} catch (error) {
    return console.error(error);
}

const include = config.include || [];
const exclude = config.exclude || [];
const output = config.output || 'types.json';

const files = diff(uniqueFilesFromGlobPatterns(include), uniqueFilesFromGlobPatterns(exclude));

exec(`dotnet run --project lib/csharp-models-to-json --files=${files.join(',')}`, (err, stdout) => {
    if (err) {
        return console.error(err);
    }

    const types = convertJsonToTypes(stdout);

    fs.writeFile(output, types, err => {
        if (err) {
            return console.error(err);
        }
    
        console.log('Done!');
    }); 
});
