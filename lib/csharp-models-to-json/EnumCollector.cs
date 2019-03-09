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
            var dic = new Dictionary<string, object>();
            foreach(var m in node.Members){
                if(m.EqualsValue == null){
                    dic[m.Identifier.ToString()] = null;
                }
                else{
                    dic[m.Identifier.ToString()] = m.EqualsValue.Value.ToString();
                }
            }

            var item = new Enum() {
                Identifier = node.Identifier.ToString(),
                Values = dic
            };
            
            this.Enums.Add(item);
        }
    }
}