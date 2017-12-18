const path = require('path');

const convertJsonToTypes = json =>
    json.map(file => {
        const rows = [];

        if (file.FileName) {
            rows.push(`// ${path.relative(process.cwd(), file.FileName)}`);
        }

        rows.push('declare module Api {');

        file.Classes.forEach(class_ => {
            rows.push(`    export interface ${class_.ClassName} {`);
            [].concat(class_.Fields, class_.Properties).forEach(member => {
                rows.push(`        ${member.Identifier}: ${member.Type};`);
            });
            rows.push(`    }`);
        });

        file.Enums.forEach(enum_ => {
            rows.push(`    export type ${enum_.Identifier} =`);
            enum_.Values.forEach((value, i) => {
                if (i === enum_.Values.length - 1) {
                    rows.push(`        '${value}'`);
                } else {
                    rows.push(`        '${value}' |`);
                }
            });
        });

        rows.push('}\n');

        return rows.join('\n');
    })
    .join('\n');

module.exports = convertJsonToTypes;
