using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace CSharpModelsToJson.Tests
{
    [TestFixture]
    public class ModelCollectorTest
    {
        [Test]
        public void BasicInheritance_ReturnsInheritedClass()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
                public class A : B, C, D
                {
                    public void AMember()
                    {
                    }
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var modelCollector = new ModelCollector();
            modelCollector.VisitClassDeclaration(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First());

            Assert.That(modelCollector.Models, Is.Not.Null);
            Assert.That(modelCollector.Models.First().BaseClasses, Is.EqualTo(new[] { "B", "C", "D" }));
        }

        [Test]
        public void InterfaceImport_ReturnsSyntaxClassFromInterface()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
                public interface IPhoneNumber {
                    string Label { get; set; }
                    string Number { get; set; }
                    int MyProperty { get; set; }
                }

                public interface IPoint
                {
                   // Property signatures:
                   int x
                   {
                      get;
                      set;
                   }

                   int y
                   {
                      get;
                      set;
                   }
                }


                public class X {
                    public IPhoneNumber test { get; set; }
                    public IPoint test2 { get; set; }
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var modelCollector = new ModelCollector();
            modelCollector.Visit(root);

            Assert.That(modelCollector.Models, Is.Not.Null);
            Assert.That(modelCollector.Models.Count, Is.EqualTo(3));
            Assert.That(modelCollector.Models.First().Properties.Count(), Is.EqualTo(3));
        }


        [Test]
        public void TypedInheritance_ReturnsInheritance()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
                public class A : IController<Controller>
                {
                    public void AMember()
                    {
                    }
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var modelCollector = new ModelCollector();
            modelCollector.VisitClassDeclaration(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First());

            Assert.That(modelCollector.Models, Is.Not.Null);
            Assert.That(modelCollector.Models.First().BaseClasses, Is.EqualTo(new[] { "IController<Controller>" }));
        }

        [Test]
        public void AccessibilityRespected_ReturnsPublicOnly()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
                public class A : IController<Controller>
                {
                    const int A_Constant = 0;

                    private string B { get; set }

                    static string C { get; set }

                    public string Included { get; set }

                    public void AMember() 
                    { 
                    }
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var modelCollector = new ModelCollector();
            modelCollector.VisitClassDeclaration(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First());

            Assert.That(modelCollector.Models, Is.Not.Null);
            Assert.That(modelCollector.Models.First().Properties, Is.Not.Null);
            Assert.That(modelCollector.Models.First().Properties.Count(), Is.EqualTo(1));
        }

        [Test]
        public void IgnoresJsonIgnored_ReturnsOnlyNotIgnored()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
                public class A : IController<Controller>
                {
                    const int A_Constant = 0;

                    private string B { get; set }

                    static string C { get; set }

                    public string Included { get; set }

                    [JsonIgnore]
                    public string Ignored { get; set; }

                    public void AMember() 
                    { 
                    }
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var modelCollector = new ModelCollector();
            modelCollector.VisitClassDeclaration(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First());

            Assert.That(modelCollector.Models, Is.Not.Null);
            Assert.That(modelCollector.Models.First().Properties, Is.Not.Null);
            Assert.That(modelCollector.Models.First().Properties.Count(), Is.EqualTo(1));

        }

        [Test]
        public void DictionaryInheritance_ReturnsIndexAccessor()
        {
            var tree = CSharpSyntaxTree.ParseText(@"public class A : Dictionary<string, string> { }");

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var modelCollector = new ModelCollector();
            modelCollector.VisitClassDeclaration(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First());

            Assert.That(modelCollector.Models, Is.Not.Null);
            Assert.That(modelCollector.Models.First().BaseClasses, Is.Not.Null);
            Assert.That(modelCollector.Models.First().BaseClasses, Is.EqualTo(new[] { "Dictionary<string, string>" }));
        }

        [Test]
        public void EnumBinaryValue()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
                public enum A
                {
                    A = 0b_0000_0001,
                    B = 0b00000010,
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var enumCollector = new EnumCollector();
            enumCollector.VisitEnumDeclaration(root.DescendantNodes().OfType<EnumDeclarationSyntax>().First());

            var model = enumCollector.Enums.First();

            Assert.IsNotNull(model);
            Assert.IsNotNull(model.Values);

            Assert.AreEqual("0b00000001", model.Values["A"]);
            Assert.AreEqual("0b00000010", model.Values["B"]);
        }

    }
}