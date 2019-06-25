# C# models to TypeScript

This is a tool that consumes your C# domain models and types and creates TypeScript declaration files from them. There's other tools that does this but what makes this one different is that it internally uses [Roslyn (the .NET compiler platform)](https://github.com/dotnet/roslyn) to parse the source files, which removes the need to create and maintain our own parser.


[![NPM version][npm-image]][npm-url]


## Dependencies

* [.NET Core SDK](https://www.microsoft.com/net/download/macos)


## Install

```
$ npm install --save csharp-models-to-typescript
```

## How to use

1. Add a config file to your project that contains for example...

```
{
    "include": [
        "./models/**/*.cs",
        "./enums/**/*.cs"
    ],
    "exclude": [
        "./models/foo/bar.cs"
    ],
    "baseTypeExclude" : [ 
        "TypeName", 
        "IInterfaceName" 
    ],
    "namespace": "Api",
    "output": "./api.d.ts",
    "camelCase": false,
    "camelCaseEnums": false,
    "numericEnums": false,
    "stringLiteralTypesInsteadOfEnums": false,
    "customTypeTranslations": {
        "ProductName": "string",
        "ProductNumber": "string"
    }
}
```

2. Add a npm script to your package.json that references your config file...

```
"scripts": {
    "generate-types": "csharp-models-to-typescript --config=your-config-file.json"
},
```

3. Run the npm script `generate-types` and the output file specified in your config should be created and populated with your models.

## Remarks

The following type translations are always used:

```
defaultTypeTranslations = {
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
```

Use `baseTypeExclude` to exclude for example `IEquatable` interface from being writen to the typescript file as this interface has no representation in typescript an would produce an error.

## License

MIT © [Jonathan Persson](https://github.com/jonathanp)

[npm-image]: https://img.shields.io/npm/v/csharp-models-to-typescript.svg
[npm-url]: https://npmjs.org/package/csharp-models-to-typescript
