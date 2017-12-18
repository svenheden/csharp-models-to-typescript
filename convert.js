const path = require('path');

const collectionRegex = /^(?:I?List|IEnumerable|ICollection|HashSet)<([\w\d]+)>\?*$/;
const dictionaryRegex = /^I?Dictionary<([\w\d]+),\s?([\w\d]+)>\?*$/;

const typeTranslations = {
    int: 'number',
    double: 'number',
    float: 'number',
    Int32: 'number',
    Int64: 'number',
    short: 'number',
    long: 'number',
    decimal: 'number',
    bool: 'boolean',
    DateTime: 'string',
    DateTimeOffset: 'string',
    Guid: 'string',
    dynamic: 'any',
    object: 'any',
};

const convertJsonToTypes = json => {
    const content = json.map(file => {
        const rows = [];

        file.Classes.forEach(class_ => {
            const members = [...class_.Fields, ...class_.Properties];

            rows.push(`    // ${path.relative(process.cwd(), file.FileName)}`);
            rows.push(`    export interface ${class_.ClassName} {`);
            members.forEach(member => {
                rows.push(convertProperty(member));
            });
            rows.push(`    }\n`);
        });

        file.Enums.forEach(enum_ => {
            rows.push(`    // ${path.relative(process.cwd(), file.FileName)}`);
            rows.push(`    export type ${enum_.Identifier} =`);
            enum_.Values.forEach((value, i) => {
                const delimiter = (i === enum_.Values.length - 1) ? ';' : ' |';
                rows.push(`        '${value}'${delimiter}`);
            });
            rows.push('');
        });

        return rows.join('\n');
    });

    return [
        'declare module Api {',
        ...content,
        '}',
    ].join('\n');
};

const convertProperty = property => {
    const optional = property.Type.endsWith('?');
    const collection = property.Type.match(collectionRegex);
    const dictionary = property.Type.match(dictionaryRegex);
    const identifier = optional ? `${property.Identifier}?` : property.Identifier;
    let type;

    if (collection) {
        type = `${convertType(collection[1])}[]`;
    } else if (dictionary) {
        type = `{ [index: ${convertType(dictionary[1])}]: ${convertType(dictionary[2])} }`
    } else {
        type = convertType(optional ? property.Type.slice(0, property.Type.length - 1) : property.Type);
    }

    return `        ${identifier}: ${type};`
}

const convertType = type => type in typeTranslations ? typeTranslations[type] : type;

module.exports = convertJsonToTypes;
