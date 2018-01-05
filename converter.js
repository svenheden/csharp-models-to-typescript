const path = require('path');

const collectionRegex = /^(?:I?List|IEnumerable|ICollection|HashSet)<([\w\d]+)>\?*$/;
const dictionaryRegex = /^I?Dictionary<([\w\d]+),\s?([\w\d]+)>\?*$/;

const defaultTypeTranslations = {
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

const createConverter = config => {
    const typeTranslations = {
        ...defaultTypeTranslations,
        ...config.customTypeTranslations
    };

    const convert = json => {
        const content = json.map(file => {
            const rows = [];

            file.Classes.forEach(class_ => {
                const members = [...class_.Fields, ...class_.Properties];

                if (members.length > 0) {
                    rows.push(`// ${path.relative(process.cwd(), file.FileName)}`);
                    rows.push(`export interface ${class_.ClassName} {`);
                    members.forEach(member => {
                        rows.push(convertProperty(member));
                    });
                    rows.push(`}\n`);
                }
            });

            file.Enums.forEach(enum_ => {
                rows.push(`// ${path.relative(process.cwd(), file.FileName)}`);
                rows.push(`export type ${enum_.Identifier} =`);
                enum_.Values.forEach((value, i) => {
                    const delimiter = (i === enum_.Values.length - 1) ? ';' : ' |';
                    rows.push(`    '${value}'${delimiter}`);
                });
                rows.push('');
            });

            if (config.namespace) {
                return rows.map(row => `    ${row}`).join('\n');
            } else {
                return rows.join('\n');
            }
        });

        const filteredContent = content.filter(x => x.length > 0);

        if (config.namespace) {
            return [
                `declare module ${config.namespace} {`,
                ...filteredContent,
                '}',
            ].join('\n');
        } else {
            return filteredContent.join('\n');
        }
    };

    const convertProperty = property => {
        const optional = property.Type.endsWith('?');
        const collection = property.Type.match(collectionRegex);
        const dictionary = property.Type.match(dictionaryRegex);
        const identifier = convertIdentifier(optional ? `${property.Identifier.split(' ')[0]}?` : property.Identifier.split(' ')[0]);

        let type;

        if (collection) {
            type = `${convertType(collection[1])}[]`;
        } else if (dictionary) {
            type = `{ [index: ${convertType(dictionary[1])}]: ${convertType(dictionary[2])} }`;
        } else {
            type = convertType(optional ? property.Type.slice(0, property.Type.length - 1) : property.Type);
        }

        return `    ${identifier}: ${type};`;
    };
    
    const convertIdentifier = identifier => config.camelCase ? identifier[0].toLowerCase() + identifier.substring(1) : identifier;
    const convertType = type => type in typeTranslations ? typeTranslations[type] : type;

    return convert;
};

module.exports = createConverter;
