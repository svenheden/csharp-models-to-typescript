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
        public string BaseClasses { get; set; }
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
        private readonly CSharpModelsToJsonOptions options;
        public ModelCollector(CSharpModelsToJsonOptions options)
        {
            this.options = options;
        }

        public readonly List<Model> Models = new List<Model>();

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var model = GetModel(node);

            Models.Add(model);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            var model = GetModel(node);

            Models.Add(model);
        }

        private Model GetModel(TypeDeclarationSyntax node)
        {
            return new Model()
            {
                ModelName = node.Identifier.ToString(),
                Fields = node.Members.OfType<FieldDeclarationSyntax>()
                                .Where(field => IsAccessible(field.Modifiers))
                                .Select(ConvertField),
                Properties = node.Members.OfType<PropertyDeclarationSyntax>()
                                .Where(property => IsAccessible(property.Modifiers))
                                .Select(ConvertProperty),
                BaseClasses = node.BaseList?.Types.ToString(),
            };
        }

        private bool IsAccessible(SyntaxTokenList modifiers) => modifiers.All(modifier =>
            modifier.ToString() != "const" &&
            modifier.ToString() != "static" &&
            modifier.ToString() != "private"
        );

        private Field ConvertField(FieldDeclarationSyntax field) => new Field
        {
            Identifier = field.Declaration.Variables.First().GetText().ToString(),
            Type = field.Declaration.Type.ToString(),
        };

        private Property ConvertProperty(PropertyDeclarationSyntax property) => new Property
        {
            Identifier = this.GetPropertyIdentifierName(property),
            Type = property.Type.ToString(),
        };

        private string GetPropertyIdentifierName(PropertyDeclarationSyntax property)
        {
            switch (this.options.PropertyNameSource)
            {
                case PropertyNameSource.JsonProperty:
                case PropertyNameSource.DataMember:
                    return GetNameFromAttributeValue(property, this.options.PropertyNameSource.ToString());
                case PropertyNameSource.Default:
                default:
                    return property.Identifier.ToString();
            }
        }

        private static string GetNameFromAttributeValue(PropertyDeclarationSyntax property, string attributeName)
        {
            var jsonPropertyAttribute = property.AttributeLists.SelectMany(attribute => attribute.Attributes)
                                    .FirstOrDefault(attribute => (attribute.Name as IdentifierNameSyntax)?.Identifier.Text ==
                                                                 attributeName);
            var nameValue = jsonPropertyAttribute?.ArgumentList.Arguments.First().Expression
                .NormalizeWhitespace().ToFullString();
            return nameValue?.Trim('"') ?? property.Identifier.ToString();
        }

    }
}