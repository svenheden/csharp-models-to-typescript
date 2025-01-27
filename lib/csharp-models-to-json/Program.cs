using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Ganss.IO;

namespace CSharpModelsToJson
{
    class File
    {
        public string FileName { get; set; }
        public IEnumerable<Model> Models { get; set; }
        public IEnumerable<Enum> Enums { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Config? config = null;
            if (System.IO.File.Exists(args[0])) {
                var configJson = System.IO.File.ReadAllText(args[0]);
                var opts = new JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true
                };
                config = JsonSerializer.Deserialize<Config>(configJson, opts);
            }

            var includes = config?.Include ?? [];
            var excludes = config?.Exclude ?? [];

            List<File> files = new List<File>();

            foreach (string fileName in getFileNames(includes, excludes)) {
                files.Add(parseFile(fileName));
            }

            JsonSerializerOptions options = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            string json = JsonSerializer.Serialize(files, options);

            var sb = new StringBuilder();
            sb.AppendLine("<<<<<<START_JSON>>>>>>");
            sb.AppendLine(json);
            sb.AppendLine("<<<<<<END_JSON>>>>>>");

            System.Console.OutputEncoding = System.Text.Encoding.UTF8;
            System.Console.WriteLine(sb.ToString());
        }

        static List<string> getFileNames(List<string> includes, List<string> excludes) {
            List<string> fileNames = new List<string>();

            foreach (var path in expandGlobPatterns(includes)) {
                fileNames.Add(path);
            }

            foreach (var path in expandGlobPatterns(excludes)) {
                fileNames.Remove(path);
            }

            return fileNames;
        }

        static List<string> expandGlobPatterns(List<string> globPatterns) {
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
 
            var modelCollector = new ModelCollector();
            var enumCollector = new EnumCollector();

            modelCollector.Visit(root);
            enumCollector.Visit(root);

            return new File() {
                FileName = System.IO.Path.GetFullPath(path),
                Models = modelCollector.Models,
                Enums = enumCollector.Enums
            };
        }
    }
}