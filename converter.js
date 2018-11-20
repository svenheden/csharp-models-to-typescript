const path = require('path');

const flatten = arr => arr.reduce((a, b) => a.concat(b), []);

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
    const typeTranslations = Object.assign({}, defaultTypeTranslations, config.customTypeTranslations);

    const convert = json => {
        const content = json.map(file => {
            const filename = path.relative(process.cwd(), file.FileName);

            const rows = flatten([
                ...file.Classes.map(class_ => convertClass(class_, filename)),
                ...file.Enums.map(enum_ => convertEnum(enum_, filename)),
            ]);

            return rows
                .map(row => config.namespace ? `    ${row}` : row)
                .join('\n');
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

    const convertClass = (class_, filename) => {
        const rows = [];
        const members = [...class_.Fields, ...class_.Properties];
        const baseClasses = class_.BaseClasses ? ` extends ${class_.BaseClasses}` : '';

        if (members.length > 0) {
            rows.push(`// ${filename}`);
            rows.push(`export interface ${class_.ClassName}${baseClasses} {`);
            members.forEach(member => {
                rows.push(convertProperty(member));
            });
            rows.push(`}\n`);
        }

        return rows;
    }

    const convertEnum = (enum_, filename) => {
        const rows = [];

        rows.push(`// ${filename}`);

        if (config.stringLiteralTypesInsteadOfEnums) {
            rows.push(`export type ${enum_.Identifier} =`);
            enum_.Values.forEach((value, i) => {
                const delimiter = (i === enum_.Values.length - 1) ? ';' : ' |';
                rows.push(`    '${value}'${delimiter}`);
            });
            rows.push('');
        } else {
            rows.push(`export enum ${enum_.Identifier} {`);
            enum_.Values.forEach(value => {
                rows.push(`    ${value} = '${value}',`);
            });
            rows.push(`}\n`);
        }

        return rows;
    }

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
