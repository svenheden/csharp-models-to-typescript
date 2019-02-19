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

        private static Model GetModel(TypeDeclarationSyntax node)
        {
            return new Model()
            {
                ModelName = $"{node.Identifier.ToString()}{node.TypeParameterList?.ToString()}",
                Fields = node.Members.OfType<FieldDeclarationSyntax>()
                                .Where(field => IsAccessible(field.Modifiers))
                                .Select(ConvertField),
                Properties = node.Members.OfType<PropertyDeclarationSyntax>()
                                .Where(property => IsAccessible(property.Modifiers))
                                .Select(ConvertProperty),
                BaseClasses = node.BaseList?.Types.ToString(),
            };
        }

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
    }
}