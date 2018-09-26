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
                ...file.Interfaces.map(interface_ => convertClass(interface_, filename))
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
        const methods = class_.Methods;

        if (members.length > 0 || (methods.length > 0 && config.includeMethods)) {
            rows.push(`// ${filename}`);
            rows.push(`export interface ${class_.ClassName}${baseClasses} {`);
            members.forEach(member => {
                rows.push(convertProperty(member));
            });
            if (config.includeMethods) {
                methods.forEach(method => {
                  rows.push(convertMethod(method));
              });
            }
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
        let type = csharpTypeToTsType(property.Type);
        const identifier = convertIdentifier(optional ? `${property.Identifier.split(' ')[0]}?` : property.Identifier.split(' ')[0]);
        return `    ${identifier}: ${type};`;
    };

    const convertMethod = method => {
        let params = method.Params
          .map(p => convertParameterType(p.Identifier, p.Type, p.Default))
          .join(', '),
          returnType = '';

        if (method.ReturnType) {
          returnType = csharpTypeToTsType(method.ReturnType);
          if (config.returnPromise === true) {
            returnType = `Promise<${returnType}>`;
          }
          returnType = `: ${returnType}`;
        }

        return `    ${method.Name} (${params})${returnType}`;
    };

    const csharpTypeToTsType = CSharpTypeType => {
      const optional = CSharpTypeType.endsWith('?');
      const collection = CSharpTypeType.match(collectionRegex);
      const dictionary = CSharpTypeType.match(dictionaryRegex);

      let type;

      if (collection) {
          type = `${convertType(collection[1])}[]`;
      } else if (dictionary) {
          type = `{ [index: ${convertType(dictionary[1])}]: ${convertType(dictionary[2])} }`;
      } else {
          type = convertType(optional ? CSharpTypeType.slice(0, CSharpTypeType.length - 1) : CSharpTypeType);
      }
      return type;
    }

    const convertParameterType = (name, parameterType, defaultParameterValue) => {
      const identifier = name;
      let type = csharpTypeToTsType(parameterType);
      var isOptional = '';
      if (defaultParameterValue) {
        isOptional = '?';
      }

      return `${identifier}${isOptional}: ${type}`;
    }

    const convertIdentifier = identifier => config.camelCase ? identifier[0].toLowerCase() + identifier.substring(1) : identifier;
    const convertType = type => type in typeTranslations ? typeTranslations[type] : type;

    return convert;
};

module.exports = createConverter;
