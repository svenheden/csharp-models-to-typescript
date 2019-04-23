const path = require('path');

const flatten = arr => arr.reduce((a, b) => a.concat(b), []);

const arrayRegex = /^(.+)\[\]$/;
const simpleCollectionRegex = /^(?:I?List|IReadOnlyList|IEnumerable|ICollection|HashSet)<([\w\d]+)>\??$/;
const collectionRegex = /^(?:I?List|IReadOnlyList|IEnumerable|ICollection|HashSet)<(.+)>\??$/;
const simpleDictionaryRegex = /^I?Dictionary<([\w\d]+)\s*,\s*([\w\d]+)>\??$/;
const dictionaryRegex = /^I?Dictionary<([\w\d]+)\s*,\s*(.+)>\??$/;

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
                ...file.Models.map(model => convertModel(model, filename)),
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

    const convertModel = (model, filename) => {
        const rows = [];

        const members = [...model.Fields, ...model.Properties];
        const baseClasses = model.BaseClasses ? ` extends ${model.BaseClasses}` : '';

        if (members.length > 0) {
            rows.push(`// ${filename}`);
            rows.push(`export interface ${model.ModelName}${baseClasses} {`);
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

        const entries = Object.entries(enum_.Values);

        if (config.stringLiteralTypesInsteadOfEnums) {
            rows.push(`export type ${enum_.Identifier} =`);

            entries.forEach(([key], i) => {
                const delimiter = (i === entries.length - 1) ? ';' : ' |';
                rows.push(`    '${key}'${delimiter}`);
            });

            rows.push('');
        } else {
            rows.push(`export enum ${enum_.Identifier} {`);

            entries.forEach(([key, value], i) => {
                if (config.numericEnums) {
                    rows.push(`    ${key} = ${value != null ? value : i},`);
                } else {
                    rows.push(`    ${key} = '${key}',`);
                }
            });

            rows.push(`}\n`);
        }

        return rows;
    }

    const convertProperty = property => {
        const optional = property.Type.endsWith('?');
        const identifier = convertIdentifier(optional ? `${property.Identifier.split(' ')[0]}?` : property.Identifier.split(' ')[0]);

        const type = parseType(property.Type);

        return `    ${identifier}: ${type};`;
    };

    const parseType = propType => {
        const array = propType.match(arrayRegex);
        if (array) {
            propType = array[1];
        }

        const collection = propType.match(collectionRegex);
        const dictionary = propType.match(dictionaryRegex);

        let type;
        
        if (collection) {
            const simpleCollection = propType.match(simpleCollectionRegex);
            propType = simpleCollection ? collection[1] : parseType(collection[1]);
            type = `${convertType(propType)}[]`;
        } else if (dictionary) {
            const simpleDictionary = propType.match(simpleDictionaryRegex);
            propType = simpleDictionary ? dictionary[2] : parseType(dictionary[2]);
            type = `{ [key: ${convertType(dictionary[1])}]: ${convertType(propType)} }`;
        } else {
            const optional = propType.endsWith('?');
            type = convertType(optional ? propType.slice(0, propType.length - 1) : propType);
        }

        return array ? `${type}[]` : type;
    };

    const convertIdentifier = identifier => config.camelCase ? identifier[0].toLowerCase() + identifier.substring(1) : identifier;
    const convertType = type => type in typeTranslations ? typeTranslations[type] : type;

    return convert;
};

module.exports = createConverter;
