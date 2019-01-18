using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace CSharpModelsToJson.Tests
{
    [TestFixture]
    public class ClassCollectorTest
    {
        [Test]
        public void BasicInheritancetest()
        {
            const string baseClasses = "B, C, D";
            var tree = CSharpSyntaxTree.ParseText(@"
                public class A : B, C, D
                {
                    public void AMember()
                    {
                    }
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var classCollector = new ClassCollector();
            classCollector.VisitClassDeclaration(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First());

            Assert.IsNotNull(classCollector.Classes);
            Assert.AreEqual(classCollector.Classes.First().BaseClasses, baseClasses);
        }

        [Test]
        public void TypedInheritanceTest()
        {
            const string baseClasses = "IController<Controller>";
            var tree = CSharpSyntaxTree.ParseText(@"
                public class A : IController<Controller>
                {
                    public void AMember()
                    {
                    }
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var classCollector = new ClassCollector();
            classCollector.VisitClassDeclaration(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First());

            Assert.IsNotNull(classCollector.Classes);
            Assert.AreEqual(classCollector.Classes.First().BaseClasses, baseClasses);
        }
        
        [Test]
        public void AccessibilityRespected()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
                public class A : IController<Controller>
                {
                    public void AMember()
                    {
                        const A_Constant = 0;
                        
                        private string B { get; set }

                        static string C { get; set }

                        public string Included { get; set } 
                    }
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var classCollector = new ClassCollector();
            classCollector.VisitClassDeclaration(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First());

            Assert.IsNotNull(classCollector.Classes);
            Assert.IsNotNull(classCollector.Classes.First().Properties);
            Assert.AreEqual(classCollector.Classes.First().Properties.Count(), 1);
        }
    }
}