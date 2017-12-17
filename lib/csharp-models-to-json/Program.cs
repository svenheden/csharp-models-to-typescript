using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace CSharpModelsToJson
{
    class File
    {
        public string FileName { get; set; }
        public IEnumerable<Class> Classes { get; set; }
        public IEnumerable<Enum> Enums { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<File> files = new List<File>();

            foreach (var argument in args) {
                if (argument.StartsWith("--files=")) {
                    var paths = argument.Substring("--files=".Length).Split(',');

                    foreach (var path in paths) {
                        files.Add(parseFile(path));
                    }
                }
            }

            var json = JsonConvert.SerializeObject(files);
            System.Console.WriteLine(json);
        }

        static File parseFile(string path) {
            string source = System.IO.File.ReadAllText(path);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            var root = (CompilationUnitSyntax) tree.GetRoot();
 
            var classCollector = new ClassCollector();
            var enumCollector = new EnumCollector();

            classCollector.Visit(root); 
            enumCollector.Visit(root);

            return new File() {
                FileName = System.IO.Path.GetFullPath(path),
                Classes = classCollector.Items,
                Enums = enumCollector.Items
            };
        }
    }
}