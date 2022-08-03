using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
 
namespace CSharpModelsToJson
{
    public class Enum
    {
        public string Identifier { get; set; }
        public ExtraInfo ExtraInfo { get; set; }
        public Dictionary<string, EnumValue> Values { get; set; }
    }

    public class EnumValue
    {
        public string Value { get; set; }
        public ExtraInfo ExtraInfo { get; set; }
    }


    public class EnumCollector: CSharpSyntaxWalker
    {
        public readonly List<Enum> Enums = new List<Enum>();

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            var values = new Dictionary<string, EnumValue>();

            foreach (var member in node.Members) {
                var equalsValue = member.EqualsValue != null
                    ? member.EqualsValue.Value.ToString()
                    : null;

                var value = new EnumValue
                {
                    Value = equalsValue?.Replace("_", ""),
                    ExtraInfo = new ExtraInfo
                    {
                        Obsolete = Util.IsObsolete(member.AttributeLists),
                        ObsoleteMessage = Util.GetObsoleteMessage(member.AttributeLists),
                        Summary = Util.GetSummaryMessage(member),
                        Remarks = Util.GetRemarksMessage(member),
                    }
                };

                values[member.Identifier.ToString()] = value;
            }

            this.Enums.Add(new Enum() {
                Identifier = node.Identifier.ToString(),
                ExtraInfo = new ExtraInfo
                {
                    Obsolete = Util.IsObsolete(node.AttributeLists),
                    ObsoleteMessage = Util.GetObsoleteMessage(node.AttributeLists),
                    Summary = Util.GetSummaryMessage(node),
                    Remarks = Util.GetRemarksMessage(node),
                },
                Values = values
            });
        }
    }
}
