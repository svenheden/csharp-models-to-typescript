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
        public void ReturnObsoleteClassInfo()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
                [Obsolete(@""test"")]
                public class A
                {
                    [Obsolete(@""test prop"")]
                    public string A { get; set }

                    public string B { get; set }
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var modelCollector = new ModelCollector();
            modelCollector.VisitClassDeclaration(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First());

            var model = modelCollector.Models.First();

            Assert.That(model, Is.Not.Null);
            Assert.That(model.Properties, Is.Not.Null);

            Assert.That(model.ExtraInfo.Obsolete, Is.True);
            Assert.That(model.ExtraInfo.ObsoleteMessage, Is.EqualTo("test"));

            Assert.That(model.Properties.First(x => x.Identifier.Equals("A")).ExtraInfo.Obsolete, Is.True);
            Assert.That(model.Properties.First(x => x.Identifier.Equals("A")).ExtraInfo.ObsoleteMessage, Is.EqualTo("test prop"));

            Assert.That(model.Properties.First(x => x.Identifier.Equals("B")).ExtraInfo.Obsolete, Is.False);
            Assert.That(model.Properties.First(x => x.Identifier.Equals("B")).ExtraInfo.ObsoleteMessage, Is.Null);
        }

        [Test]
        public void ReturnObsoleteEnumInfo()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
                [Obsolete(@""test"")]
                public enum A
                {
                    A = 0,
                    B = 1,
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var enumCollector = new EnumCollector();
            enumCollector.VisitEnumDeclaration(root.DescendantNodes().OfType<EnumDeclarationSyntax>().First());

            var model = enumCollector.Enums.First();

            Assert.That(model, Is.Not.Null) ;
            Assert.That(model.Values, Is.Not.Null);

            Assert.That(model.ExtraInfo.Obsolete, Is.True);
            Assert.That(model.ExtraInfo.ObsoleteMessage, Is.EqualTo("test"));
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

            Assert.That(model.Values["A"].Value, Is.EqualTo("1"));
            Assert.That(model.Values["B"].Value, Is.EqualTo("1002"));
            Assert.That(model.Values["C"].Value, Is.EqualTo("0b011"));
            Assert.That(model.Values["D"].Value, Is.EqualTo("0b00000100"));
            Assert.That(model.Values["E"].Value, Is.EqualTo("0x005"));
            Assert.That(model.Values["F"].Value, Is.EqualTo("0x00001a"));
        }

        [Test]
        public void ReturnEmmitDefaultValueInfo()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
                public class A
                {
                    [DataMember(EmitDefaultValue = false)]
                    public bool Prop1 { get; set; }

                    [DataMember(EmitDefaultValue = true)]
                    public bool Prop2 { get; set; }

                    [DataMember( EmitDefaultValue = false )]
                    public bool Prop3 { get; set; }

                    [DataMember]
                    public bool Prop4 { get; set; }

                    public bool Prop5 { get; set; }
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var modelCollector = new ModelCollector();
            modelCollector.Visit(root);

            Assert.That(modelCollector.Models, Is.Not.Null);
            Assert.That(modelCollector.Models.Count, Is.EqualTo(1));
            
            var properties = modelCollector.Models.First().Properties;

            Assert.That(properties.First(x => x.Identifier == "Prop1").ExtraInfo.EmitDefaultValue, Is.False);
            Assert.That(properties.First(x => x.Identifier == "Prop2").ExtraInfo.EmitDefaultValue, Is.True);
            Assert.That(properties.First(x => x.Identifier == "Prop3").ExtraInfo.EmitDefaultValue, Is.False);
            Assert.That(properties.First(x => x.Identifier == "Prop4").ExtraInfo.EmitDefaultValue, Is.True);
            Assert.That(properties.First(x => x.Identifier == "Prop5").ExtraInfo.EmitDefaultValue, Is.True);
        }

    }
}