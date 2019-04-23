using System.Collections.Generic;
using System.Linq;
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

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            var values = new Dictionary<string, object>();

            foreach (var member in node.Members) {
                values[member.Identifier.ToString()] = member.EqualsValue != null
                    ? member.EqualsValue.Value.ToString()
                    : null;
            }

            this.Enums.Add(new Enum() {
                Identifier = node.Identifier.ToString(),
                Values = values
            });
        }
    }
}
