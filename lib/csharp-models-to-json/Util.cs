using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
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
                {
                    return obsoleteAttribute.ArgumentList == null
                            ? null
                            : obsoleteAttribute.ArgumentList.Arguments.ToString()?.TrimStart('@').Trim('"');
                }
            }

            return null;
        }

        internal static string GetSummaryMessage(SyntaxNode @class)
        {
            var documentComment = @class.GetDocumentationCommentTriviaSyntax();

            if (documentComment == null)
                return null;

            var summaryElement = documentComment.Content
               .OfType<XmlElementSyntax>()
               .FirstOrDefault(_ => _.StartTag.Name.LocalName.Text == "summary");

            if (summaryElement == null)
                return null;

            var summaryText = summaryElement.DescendantTokens()
                .Where(_ => _.Kind() == SyntaxKind.XmlTextLiteralToken)
                .Select(_ => _.Text.Trim());

            //var text = documentComment.GetXmlTextSyntax();
            
            return string.Join(Environment.NewLine, summaryText).Trim();
        }

        public static DocumentationCommentTriviaSyntax GetDocumentationCommentTriviaSyntax(this SyntaxNode node)
        {
            if (node == null)
            {
                return null;
            }

            foreach (var leadingTrivia in node.GetLeadingTrivia())
            {
                var structure = leadingTrivia.GetStructure() as DocumentationCommentTriviaSyntax;

                if (structure != null)
                {
                    return structure;
                }
            }

            return null;
        }
    }
}
