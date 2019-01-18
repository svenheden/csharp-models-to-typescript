using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpModelsToJson
{
    class Enum
    {
        public string Identifier { get; set; }
        public IEnumerable<string> Values { get; set; }
    }

    class EnumCollector: CSharpSyntaxWalker
    {
        public readonly List<Enum> Enums = new List<Enum>();

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            var item = new Enum() {
                Identifier = node.Identifier.ToString(),
                Values = node.Members.Select(val => val.Identifier.ToString())
            };

            this.Enums.Add(item);
        }
    }
}