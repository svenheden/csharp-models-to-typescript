using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace CSharpModelsToJson
{
    internal static class Util
    {
        internal static bool IsObsolete(SyntaxList<AttributeListSyntax> attributeLists) =>
            attributeLists.Any(attributeList =>
                attributeList.Attributes.Any(attribute =>
                    attribute.Name.ToString().Equals("Obsolete") || attribute.Name.ToString().Equals("ObsoleteAttribute")));

        internal static string GetObsoleteMessage(SyntaxList<AttributeListSyntax> attributeLists)
        {
            foreach (var attributeList in attributeLists)
            {
                var obsoleteAttribute =
                    attributeList.Attributes.FirstOrDefault(attribute =>
                        attribute.Name.ToString().Equals("Obsolete") || attribute.Name.ToString().Equals("ObsoleteAttribute"));

                if (obsoleteAttribute != null)
                    return obsoleteAttribute.ArgumentList == null
                        ? null
                        : obsoleteAttribute.ArgumentList.Arguments.ToString()?.TrimStart('@').Trim('"');
            }

            return null;
        }
    }
}
