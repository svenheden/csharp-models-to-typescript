using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CSharpModelsToJson.Tests
{
    [TestFixture]
    public class MethodsTest
    {
        [Test]
        public void BasicMethodInInterface()
        {

            var tree = CSharpSyntaxTree.ParseText(@"
                public interface Cheese
                {
                    public void Consume();
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();
            var interfaceCollector = new InterfaceCollector();

            interfaceCollector.Visit(root);

            Assert.AreEqual(interfaceCollector.Items.Count, 1);
            Assert.AreEqual(interfaceCollector.Items.First().ClassName, "Cheese");
            Assert.AreEqual(interfaceCollector.Items.First().Methods.Count(), 1);
            Assert.AreEqual(interfaceCollector.Items.First().Methods.First().Name, "Consume");
            Assert.AreEqual(interfaceCollector.Items.First().Methods.First().ReturnType, "void");
            Assert.AreEqual(interfaceCollector.Items.First().Methods.First().Params.Any(), false);

        }

        [Test]
        public void BasicMethodInClass()
        {

            var tree = CSharpSyntaxTree.ParseText(@"
                public class Cheese
                {
                    public void Consume() {}
                    private void Dispose() {}
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();
            var classCollector = new ClassCollector();

            classCollector.Visit(root);

            Assert.AreEqual(classCollector.Items.Count, 1);
            Assert.AreEqual(classCollector.Items.First().ClassName, "Cheese");
            Assert.AreEqual(classCollector.Items.First().Methods.Count(), 1);
            Assert.AreEqual(classCollector.Items.First().Methods.First().Name, "Consume");
            Assert.AreEqual(classCollector.Items.First().Methods.First().ReturnType, "void");
            Assert.AreEqual(classCollector.Items.First().Methods.First().Params.Any(), false);

        }

        [Test]
        public void BasicMethodInInterfaceIntParam()
        {

            var tree = CSharpSyntaxTree.ParseText(@"
                public interface Cheese
                {
                    public void Consume(int mouthfuls);
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();
            var interfaceCollector = new InterfaceCollector();

            interfaceCollector.Visit(root);

            Assert.AreEqual(interfaceCollector.Items.Count, 1);
            Assert.AreEqual(interfaceCollector.Items.First().ClassName, "Cheese");
            Assert.AreEqual(interfaceCollector.Items.First().Methods.Count(), 1);
            Assert.AreEqual(interfaceCollector.Items.First().Methods.First().Name, "Consume");
            Assert.AreEqual(interfaceCollector.Items.First().Methods.First().ReturnType, "void");
            Assert.AreEqual(interfaceCollector.Items.First().Methods.First().Params.Any(param => param.Identifier == "mouthfuls" && param.Type == "int"), true);

        }

        [Test]
        public void BasicMethodInInterfaceOptionalIntParam()
        {

            var tree = CSharpSyntaxTree.ParseText(@"
                public interface Cheese
                {
                    public void Consume(int mouthfuls = 0);
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();
            var interfaceCollector = new InterfaceCollector();

            interfaceCollector.Visit(root);

            Assert.AreEqual(interfaceCollector.Items.Count, 1);
            Assert.AreEqual(interfaceCollector.Items.First().ClassName, "Cheese");
            Assert.AreEqual(interfaceCollector.Items.First().Methods.Count(), 1);
            Assert.AreEqual(interfaceCollector.Items.First().Methods.First().Name, "Consume");
            Assert.AreEqual(interfaceCollector.Items.First().Methods.First().ReturnType, "void");
            Assert.AreEqual(interfaceCollector.Items.First().Methods.First().Params.Any(param => param.Identifier == "mouthfuls" && param.Type == "int" && param.Default != null), true);

        }

        [Test]
        public void BasicMethodInInterfaceReturnGuid()
        {

            var tree = CSharpSyntaxTree.ParseText(@"
                public interface Cheese
                {
                    public Guid Consume();
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();
            var interfaceCollector = new InterfaceCollector();

            interfaceCollector.Visit(root);

            Assert.AreEqual(interfaceCollector.Items.Count, 1);
            Assert.AreEqual(interfaceCollector.Items.First().ClassName, "Cheese");
            Assert.AreEqual(interfaceCollector.Items.First().Methods.Count(), 1);
            Assert.AreEqual(interfaceCollector.Items.First().Methods.First().Name, "Consume");
            Assert.AreEqual(interfaceCollector.Items.First().Methods.First().ReturnType, "Guid");

        }

        [Test]
        public void BasicMethodInInterfaces()
        {

            var tree = CSharpSyntaxTree.ParseText(@"
                public interface Cheese
                {
                    public Guid Consume();
                }

                public interface Tea
                {
                    public void Drink(bool isNoon);
                }
"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();
            var interfaceCollector = new InterfaceCollector();

            interfaceCollector.Visit(root);

            Assert.AreEqual(interfaceCollector.Items.Count, 2);
            Assert.AreEqual(interfaceCollector.Items.First().ClassName, "Cheese");
            Assert.AreEqual(interfaceCollector.Items.First().Methods.Count(), 1);
            Assert.AreEqual(interfaceCollector.Items.First().Methods.First().Name, "Consume");
            Assert.AreEqual(interfaceCollector.Items.First().Methods.First().Params.Count(), 0);

            Assert.AreEqual(interfaceCollector.Items.Last().ClassName, "Tea");
            Assert.AreEqual(interfaceCollector.Items.Last().Methods.Count(), 1);
            Assert.AreEqual(interfaceCollector.Items.Last().Methods.First().Name, "Drink");
            Assert.AreEqual(interfaceCollector.Items.Last().Methods.First().Params.Any(param => param.Identifier == "isNoon" && param.Type == "bool"), true);

        }


    }

}
