using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
 
namespace CSharpModelsToJson
{
    class Class
    {
        public string ClassName { get; set; }
        public IEnumerable<Field> Fields { get; set; }
        public IEnumerable<Property> Properties { get; set; }
    }

    class Field
    {
        public string Identifier { get; set; }
        public string Type { get; set; }
    }

    class Property
    {
        public string Identifier { get; set; }
        public string Type { get; set; }
    }

    class ClassCollector: CSharpSyntaxWalker
    {
        public readonly List<Class> Items = new List<Class>();

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var item = new Class() {
                ClassName = node.Identifier.ToString(),
                Fields = node.Members.OfType<FieldDeclarationSyntax>()
                    .Select(field => {
                        var declaration = field.Declaration.ToString().Split(' ');

                        return new Field {
                            Identifier = declaration[1],
                            Type = declaration[0],
                        };
                    }),
                Properties = node.Members.OfType<PropertyDeclarationSyntax>()
                    .Select(property => new Property {
                        Identifier = property.Identifier.ToString(),
                        Type = property.Type.ToString(),
                    })
            };
            
            this.Items.Add(item);
        }
    }
}