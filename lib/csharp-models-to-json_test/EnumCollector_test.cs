using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace CSharpModelsToJson.Tests
{
    [TestFixture]
    public class EnumCollectorTest
    {
        [Test]
        public void ReturnEnumWithMissingValues()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
                public enum SampleEnum
                {
                   A,
                   B = 7,
                   C,
                   D = 4,
                   E
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var enumCollector = new EnumCollector();
            enumCollector.VisitEnumDeclaration(root.DescendantNodes().OfType<EnumDeclarationSyntax>().First());

            var model = enumCollector.Enums.First();

            Assert.That(model, Is.Not.Null);
            Assert.That(model.Values, Is.Not.Null);

            Assert.That(model.Values["A"].Value, Is.Null);
            Assert.That(model.Values["B"].Value, Is.EqualTo("7"));
            Assert.That(model.Values["C"].Value, Is.Null);
            Assert.That(model.Values["D"].Value, Is.EqualTo("4"));
            Assert.That(model.Values["E"].Value, Is.Null);
        }
    }
}