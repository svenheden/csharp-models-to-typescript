using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpModelsToJson
{
    public class Class
    {
        public string ClassName { get; set; }
        public IEnumerable<Field> Fields { get; set; }
        public IEnumerable<Property> Properties { get; set; }
        public IEnumerable<Method> Methods { get; set; }
        public string BaseClasses { get; set; }
    }

    public class Field
    {
        public string Identifier { get; set; }
        public string Type { get; set; }
    }

    public class Property
    {
        public string Identifier { get; set; }
        public string Type { get; set; }
    }

    public class ClassCollector : CSharpSyntaxWalker
    {
        public readonly List<Class> Items = new List<Class>();

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var item = new Class() {
                ClassName = node.Identifier.ToString(),
                Fields = node.Members.OfType<FieldDeclarationSyntax>()
                    .Where(field => !field.Modifiers.Any(modifier => modifier.ToString() == "const" || modifier.ToString() == "static" || modifier.ToString() == "private"))
                    .Select(field => new Field {
                        Identifier = field.Declaration.Variables.First().GetText().ToString(),
                        Type = field.Declaration.Type.ToString(),
                    }),
                Methods = node.Members.OfType<MethodDeclarationSyntax>()
          .Where(method => !method.Modifiers.Any(modifier => modifier.ToString() == "private" || modifier.ToString() == "static"))
          .Select(method => new Method
          {
              Name = method.Identifier.Text,
              ReturnType = method.ReturnType.ToString(),
              Params = method.ParameterList.Parameters.Select(parameter => new Parameter
              {
                  Identifier = parameter.Identifier.Text,
                  Type = parameter.Type.ToString(),
                  Default = parameter.Default != null ? new DefaultValue
                  {
                      Value = parameter.Default.Value.GetLastToken().Value != null ? parameter.Default.Value.GetLastToken().Value.ToString() : parameter.Default.Value.GetLastToken().ValueText,
                      IsNull = parameter.Default.Value.GetLastToken().Kind().ToString() == "NullKeyword"
                  } : null
              })
          }),
                Properties = node.Members.OfType<PropertyDeclarationSyntax>()
                    .Where(property => !property.Modifiers.Any(modifier => modifier.ToString() == "const" || modifier.ToString() == "static" || modifier.ToString() == "private"))
                    .Select(property => new Property {
                        Identifier = property.Identifier.ToString(),
                        Type = property.Type.ToString(),
                    }),
                BaseClasses = node.BaseList?.Types.ToString(),
            };

            this.Items.Add(item);
        }
    }
}