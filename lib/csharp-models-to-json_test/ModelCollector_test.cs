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

                    [IgnoreDataMember]
                    public string Ignored2 { get; set; }

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
                public enum A {
                    A = 1,              // decimal: 1
                    B = 1_002,          // decimal: 1002
                    C = 0b011,          // binary: 3 in decimal
                    D = 0b_0000_0100,   // binary: 4 in decimal
                    E = 0x005,          // hexadecimal: 5 in decimal
                    F = 0x000_01a,      // hexadecimal: 26 in decimal
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var enumCollector = new EnumCollector();
            enumCollector.VisitEnumDeclaration(root.DescendantNodes().OfType<EnumDeclarationSyntax>().First());

            var model = enumCollector.Enums.First();

            Assert.That(model, Is.Not.Null);
            Assert.That(model.Values, Is.Not.Null);

            Assert.That(model.Values["A"], Is.EqualTo("1"));
            Assert.That(model.Values["B"], Is.EqualTo("1002"));
            Assert.That(model.Values["C"], Is.EqualTo("0b011"));
            Assert.That(model.Values["D"], Is.EqualTo("0b00000100"));
            Assert.That(model.Values["E"], Is.EqualTo("0x005"));
            Assert.That(model.Values["F"], Is.EqualTo("0x00001a"));
        }

    }
}