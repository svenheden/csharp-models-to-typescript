using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
 
namespace CSharpModelsToJson
{
    class Enum
    {
        public string Identifier { get; set; }
        public Dictionary<string, object> Values { get; set; }
    }

    class EnumCollector: CSharpSyntaxWalker
    {
        public readonly List<Enum> Enums = new List<Enum>();
        public readonly string IncludeAttribute = "";

        public EnumCollector(string onlyWhenAttributed)
        {
            IncludeAttribute = onlyWhenAttributed;
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            if (Include(node.AttributeLists, IncludeAttribute))
            {
                var values = new Dictionary<string, object>();

                foreach (var member in node.Members)
                {
                    values[member.Identifier.ToString()] = member.EqualsValue != null
                        ? member.EqualsValue.Value.ToString()
                        : null;
                }

                this.Enums.Add(new Enum()
                {
                    Identifier = node.Identifier.ToString(),
                    Values = values
                });
            }
        }

        private static bool Include(SyntaxList<AttributeListSyntax> propertyAttributeLists, string includeAttribute) =>
            string.IsNullOrEmpty(includeAttribute)
            || propertyAttributeLists.Any(attributeList =>
                attributeList.Attributes.Any(attribute =>
                    attribute.Name.ToString().Equals(includeAttribute)));
    }
}
