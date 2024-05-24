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

            var values = model.Values.ToArray();
            Assert.That(values[0].Value, Is.Null);
            Assert.That(values[1].Value, Is.EqualTo("7"));
            Assert.That(values[2].Value, Is.Null);
            Assert.That(values[3].Value, Is.EqualTo("4"));
            Assert.That(values[4].Value, Is.Null);
        }
    }
}