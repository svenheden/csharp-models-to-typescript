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
        public ExtraInfo ExtraInfo { get; set; }
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
        public ExtraInfo ExtraInfo { get; set; }
    }

    public class ModelCollector : CSharpSyntaxWalker
    {
        public readonly List<Model> Models = new List<Model>();

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var model = CreateModel(node);

            Models.Add(model);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            var model = CreateModel(node);

            Models.Add(model);
        }

        public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
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

        private static Model CreateModel(TypeDeclarationSyntax node)
        {
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
                BaseClasses = node.BaseList?.Types.Select(s => s.ToString()),
                ExtraInfo = new ExtraInfo
                {
                    Obsolete = Util.IsObsolete(node.AttributeLists),
                    ObsoleteMessage = Util.GetObsoleteMessage(node.AttributeLists),
                    Summary = Util.GetSummaryMessage(node),
                    Remarks = Util.GetRemarksMessage(node),
                }
            };
        }

        private static bool IsIgnored(SyntaxList<AttributeListSyntax> propertyAttributeLists) => 
            propertyAttributeLists.Any(attributeList => 
                attributeList.Attributes.Any(attribute => 
                    attribute.Name.ToString().Equals("JsonIgnore") ||
                    attribute.Name.ToString().Equals("IgnoreDataMember")));

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
            ExtraInfo = new ExtraInfo
            {
                Obsolete = Util.IsObsolete(property.AttributeLists),
                ObsoleteMessage = Util.GetObsoleteMessage(property.AttributeLists),
                Summary = Util.GetSummaryMessage(property),
                Remarks = Util.GetRemarksMessage(property),
                EmitDefaultValue = Util.GetEmitDefaultValue(property.AttributeLists),
            }
        };
    }
}