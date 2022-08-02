using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
 
namespace CSharpModelsToJson
{
    public class Enum
    {
        public string Identifier { get; set; }
        public ExtraInfo ExtraInfo { get; set; }
        public IEnumerable<EnumValue> Values { get; set; }
    }

    public class EnumValue
    {
        public string Identifier { get; set; }
        public string Value { get; set; }
        public ExtraInfo ExtraInfo { get; set; }
    }


    public class EnumCollector: CSharpSyntaxWalker
    {
        public readonly List<Enum> Enums = new List<Enum>();

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            var values = new List<EnumValue>();

            foreach (var member in node.Members) {
                var value = new EnumValue
                {
                    Identifier = member.Identifier.ToString(),
                    Value = member.EqualsValue != null
                        ? member.EqualsValue.Value.ToString()
                        : null,
                    ExtraInfo = new ExtraInfo
                    {
                        Obsolete = Util.IsObsolete(member.AttributeLists),
                        ObsoleteMessage = Util.GetObsoleteMessage(member.AttributeLists),
                        Summary = Util.GetSummaryMessage(member),
                    }
                };

                values.Add(value);
            }

            this.Enums.Add(new Enum() {
                Identifier = node.Identifier.ToString(),
                ExtraInfo = new ExtraInfo
                {
                    Obsolete = Util.IsObsolete(node.AttributeLists),
                    ObsoleteMessage = Util.GetObsoleteMessage(node.AttributeLists),
                    Summary = Util.GetSummaryMessage(node),
                },
                Values = values
            });
        }
    }
}
