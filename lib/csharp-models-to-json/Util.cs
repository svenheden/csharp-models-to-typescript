using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Text.RegularExpressions;

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

        internal static string GetSummaryMessage(SyntaxNode classItem)
        {
            return GetCommentTag(classItem, "summary");
        }

        internal static string GetRemarksMessage(SyntaxNode classItem)
        {
            return GetCommentTag(classItem, "remarks");
        }

        private static string GetCommentTag(SyntaxNode classItem, string xmlTag)
        {
            var documentComment = classItem.GetDocumentationCommentTriviaSyntax();

            if (documentComment == null)
                return null;

            var summaryElement = documentComment.Content
               .OfType<XmlElementSyntax>()
               .FirstOrDefault(_ => _.StartTag.Name.LocalName.Text == xmlTag);

            if (summaryElement == null)
                return null;

            var summaryText = summaryElement.DescendantTokens()
                .Where(_ => _.Kind() == SyntaxKind.XmlTextLiteralToken)
                .Select(_ => _.Text.Trim())
                .ToList();

            var summaryContent = summaryElement.Content.ToString();
            summaryContent = Regex.Replace(summaryContent, @"^\s*///\s*", string.Empty, RegexOptions.Multiline);
            summaryContent = Regex.Replace(summaryContent, "^<para>", Environment.NewLine, RegexOptions.Multiline);
            summaryContent = Regex.Replace(summaryContent, "</para>", string.Empty);

            return summaryContent.Trim();
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

        internal static bool GetEmitDefaultValue(SyntaxList<AttributeListSyntax> attributeLists)
        {
            var dataMemberAttribute = attributeLists
                .SelectMany(attributeList => attributeList.Attributes)
                .FirstOrDefault(attribute => attribute.Name.ToString().Equals("DataMember") || attribute.Name.ToString().Equals("DataMemberAttribute"));

            if (dataMemberAttribute?.ArgumentList == null)
                return true;

            var emitDefaultValueArgument = dataMemberAttribute.ArgumentList.Arguments.FirstOrDefault(x => x.ToString().StartsWith("EmitDefaultValue"));

            if (emitDefaultValueArgument == null)
                return true;

            return !emitDefaultValueArgument.ToString().EndsWith("false");
        }
    }
}
