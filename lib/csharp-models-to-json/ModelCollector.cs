﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpModelsToJson
{
    public class Model
    {
        public string ModelName { get; set; }
        public IEnumerable<Field> Fields { get; set; }
        public IEnumerable<Property> Properties { get; set; }
        public IEnumerable<string> BaseClasses { get; set; }
        public Dictionary<string, object> Enumerations { get; set; }
    }

    public class Field
    {
        public string Identifier { get; set; }
        public string Type { get; set; }
    }

    public class Property
    {
        public string Identifier { get; set; }
        public string Type { get; set; }
    }

    public class ModelCollector : CSharpSyntaxWalker
    {
        public readonly List<Model> Models = new List<Model>();
        public readonly string IncludeAttribute = "";

        public ModelCollector(string onlyWhenAttributed)
        {
            IncludeAttribute = onlyWhenAttributed;
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (Include(node.AttributeLists, IncludeAttribute))
            {
                var model = CreateModel(node);

                Models.Add(model);
            }
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            if (Include(node.AttributeLists, IncludeAttribute))
            {
                var model = CreateModel(node);

                Models.Add(model);
            }
        }

        public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
        {
            if (Include(node.AttributeLists, IncludeAttribute))
            {
                var model = new Model()
                {
                    ModelName = $"{node.Identifier.ToString()}{node.TypeParameterList?.ToString()}",
                    Fields = node.ParameterList?.Parameters
                                .Where(field => IsAccessible(field.Modifiers))
                                .Where(property => !IsIgnored(property.AttributeLists))
                                .Select((field) => new Field
                                {
                                    Identifier = field.Identifier.ToString(),
                                    Type = field.Type.ToString(),
                                }),
                    Properties = node.Members.OfType<PropertyDeclarationSyntax>()
                                .Where(property => IsAccessible(property.Modifiers))
                                .Where(property => !IsIgnored(property.AttributeLists))
                                .Select(ConvertProperty),
                    BaseClasses = new List<string>(),
                };

                Models.Add(model);
            }
        }

        private static Model CreateModel(TypeDeclarationSyntax node)
        {
            IEnumerable<string> baseClasses = node.BaseList?.Types.Select(s => s.ToString());
            return new Model()
            {
                ModelName = $"{node.Identifier.ToString()}{node.TypeParameterList?.ToString()}",
                Fields = node.Members.OfType<FieldDeclarationSyntax>()
                                .Where(field => IsAccessible(field.Modifiers))
                                .Where(property => !IsIgnored(property.AttributeLists))
                                .Select(ConvertField),
                Properties = node.Members.OfType<PropertyDeclarationSyntax>()
                                .Where(property => IsAccessible(property.Modifiers))
                                .Where(property => !IsIgnored(property.AttributeLists))
                                .Select(ConvertProperty),
                BaseClasses = baseClasses,
                Enumerations = baseClasses != null && baseClasses.Contains("Enumeration")
                                ? ConvertEnumerations(node.Members.OfType<FieldDeclarationSyntax>()
                                    .Where(property => !IsIgnored(property.AttributeLists)))
                                : null,
            };
        }

        private static bool Include(SyntaxList<AttributeListSyntax> propertyAttributeLists, string includeAttribute) =>
            string.IsNullOrEmpty(includeAttribute)
            || propertyAttributeLists.Any(attributeList =>
                attributeList.Attributes.Any(attribute =>
                    attribute.Name.ToString().Equals(includeAttribute)));

        private static bool IsIgnored(SyntaxList<AttributeListSyntax> propertyAttributeLists) => 
            propertyAttributeLists.Any(attributeList => 
                attributeList.Attributes.Any(attribute => 
                    attribute.Name.ToString().Equals("JsonIgnore")));

        private static bool IsAccessible(SyntaxTokenList modifiers) => modifiers.All(modifier =>
            modifier.ToString() != "const" &&
            modifier.ToString() != "static" &&
            modifier.ToString() != "private"
        );

        private static Field ConvertField(FieldDeclarationSyntax field) => new Field
        {
            Identifier = field.Declaration.Variables.First().GetText().ToString(),
            Type = field.Declaration.Type.ToString(),
        };

        private static Property ConvertProperty(PropertyDeclarationSyntax property) => new Property
        {
            Identifier = property.Identifier.ToString(),
            Type = property.Type.ToString(),
        };

        private static Dictionary<string, object> ConvertEnumerations(IEnumerable<FieldDeclarationSyntax> fields)
        {
            var values = new Dictionary<string, object>();

            foreach (FieldDeclarationSyntax field in fields)
            {
                VariableDeclaratorSyntax variable = field.Declaration.Variables.First();
                List<SyntaxToken> tokens = variable.DescendantTokens().ToList();

                string idValue = tokens.Count > 4 ? tokens[4].Value.ToString() : null;
                if (idValue == "id" && tokens.Count > 6)
                {
                    idValue = tokens[6].Value.ToString();
                }

                values[variable.GetFirstToken().ToString()] = idValue;
            }

            return values;
        }
    }
}