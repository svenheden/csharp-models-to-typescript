using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Ganss.IO;

namespace CSharpModelsToJson
{
    class File
    {
        public string FileName { get; set; }
        public IEnumerable<Class> Classes { get; set; }
        public IEnumerable<Enum> Enums { get; set; }
        public IEnumerable<Interface> Interfaces { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<File> files = new List<File>();

            foreach (string fileName in getFileNames(args)) {
                files.Add(parseFile(fileName));
            }

            string json = JsonConvert.SerializeObject(files);
            System.Console.WriteLine(json);
        }

        static List<string> getFileNames(string[] args) {
            List<string> fileNames = new List<string>();

            foreach (string arg in args) {
                if (arg.StartsWith("--include=")) {
                    string[] globPatterns = arg.Substring("--include=".Length).Split(';');

                    foreach (var path in expandGlobPatterns(globPatterns)) {
                        fileNames.Add(path);
                    }
                } else if (arg.StartsWith("--exclude=")) {
                    string[] globPatterns = arg.Substring("--exclude=".Length).Split(';');

                    foreach (var path in expandGlobPatterns(globPatterns)) {
                        fileNames.Remove(path);
                    }
                }
            }

            return fileNames;
        }

        static List<string> expandGlobPatterns(string[] globPatterns) {
            List<string> fileNames = new List<string>();

            foreach (string pattern in globPatterns) {
                var paths = Glob.Expand(pattern);

                foreach (var path in paths) {
                    fileNames.Add(path.FullName);
                }
            }

            return fileNames;
        }

        static File parseFile(string path) {
            string source = System.IO.File.ReadAllText(path);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            var root = (CompilationUnitSyntax) tree.GetRoot();
 
            var classCollector = new ClassCollector();
            var enumCollector = new EnumCollector();
            var interfaceCollector = new InterfaceCollector();

            classCollector.Visit(root); 
            enumCollector.Visit(root);
            interfaceCollector.Visit(root);

            return new File() {
                FileName = System.IO.Path.GetFullPath(path),
                Classes = classCollector.Items,
                Enums = enumCollector.Items,
                Interfaces = interfaceCollector.Items
            };
        }
    }
}
