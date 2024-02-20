using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
 
namespace CSharpModelsToJson
{
    public class Enum
    {
        public string Identifier { get; set; }
        public Dictionary<string, object> Values { get; set; }
    }

    public class EnumCollector: CSharpSyntaxWalker
    {
        public readonly List<Enum> Enums = new List<Enum>();

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            var values = new Dictionary<string, object>();

            foreach (var member in node.Members) {
                var value = member.EqualsValue != null
                    ? member.EqualsValue.Value.ToString()
                    : null;

                if (value?.StartsWith("0b") == true)
                    value = value.Replace("_", "");

                values[member.Identifier.ToString()] = value;
            }

            this.Enums.Add(new Enum() {
                Identifier = node.Identifier.ToString(),
                Values = values
            });
        }
    }
}
