using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;

namespace Atlantis.Grpc.Utilies
{
    public class CodeBuilder
    {
        private static readonly CodeBuilder _instance = new CodeBuilder();
        private readonly IList<CodeClass> _classes = new List<CodeClass>();
        private readonly IList<string> _assemblyRefenceDLLs = new List<string>();
        private readonly IList<string> _refences = new List<string>();
        private string _defaultNamespaces = "";

        private const string FileName = "Atlantis.Grpc.Generation";

        private CodeBuilder()
        {
            _refences.Add("using System;");
            _refences.Add("using System.Collections.Generic;");
        }

        public static CodeBuilder Instance => _instance;

        public CodeClass CreateClass(
            string className, string[] bastTypes = null, string namespaces = null)
        {
            var codeClass = new CodeClass(className, namespaces ?? _defaultNamespaces, this, bastTypes);
            _classes.Add(codeClass);
            return codeClass;
        }

        public CodeBuilder AddAssemblyRefence(params string[] refenceAssemblyNamesOrFiles)
        {
            if (refenceAssemblyNamesOrFiles == null || refenceAssemblyNamesOrFiles.Length == 0) return this;
            foreach (var assembly in refenceAssemblyNamesOrFiles)
            {
                if (_assemblyRefenceDLLs.Contains(assembly)) continue;
                _assemblyRefenceDLLs.Add(assembly);
            }
            return this;
        }

        public CodeBuilder AddRefence(params string[] refences)
        {
            if (refences == null || refences.Length == 0) return this;
            foreach (var item in refences)
            {
                if (_refences.Contains(item)) continue;
                _refences.Add(item);
            }
            return this;
        }

        public CodeAssembly Build()
        {
            var code = new StringBuilder();
            foreach (var item in _refences) code.AppendLine(item);
            foreach (var item in _classes) code.AppendLine(item.ToString());

            var codePath =Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location) + "/" + FileName + ".cs";
            if (File.Exists(codePath)) File.Delete(codePath);
            File.WriteAllText(codePath, code.ToString());

            var dllPath = Path.Combine(Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location),$"{FileName}.dll");
            var pdbPath = Path.Combine(Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location),$"{FileName}.pdb");
            if(File.Exists(dllPath))File.Delete(dllPath);
            var sysdllDirectory=Path.GetDirectoryName( typeof(object).Assembly.Location);
            var tree = SyntaxFactory.ParseSyntaxTree(code.ToString());
            // A single, immutable invocation to the compiler
            // to produce a library
            var compilation = CSharpCompilation.Create($"{FileName}.dll")
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
              .AddSyntaxTrees(tree)
              .AddReferences(MetadataReference.CreateFromFile($"{sysdllDirectory}/System.Private.CoreLib.dll"))
              .AddReferences(MetadataReference.CreateFromFile($"{sysdllDirectory}/System.Runtime.dll"))
              .AddReferences(MetadataReference.CreateFromFile($"{sysdllDirectory}/netstandard.dll"))
              .AddReferences(MetadataReference.CreateFromFile($"{sysdllDirectory}/System.Threading.Tasks.dll"))
              .AddReferences(MetadataReference.CreateFromFile($"{sysdllDirectory}/System.ComponentModel.dll"));
            foreach (var item in _assemblyRefenceDLLs) compilation=compilation.AddReferences(MetadataReference.CreateFromFile(item));

            var compilationResult = compilation.Emit(dllPath);
            if (!compilationResult.Success)
            {
                var issues = new StringBuilder();
                foreach (Diagnostic codeIssue in compilationResult.Diagnostics)
                {
                    issues.AppendLine($@"ID: {codeIssue.Id}, Message: {codeIssue.GetMessage()},
                                        Location: { codeIssue.Location.GetLineSpan()},
                                        Severity: { codeIssue.Severity}
                                                ");
                }
                throw new InvalidOperationException(issues.ToString());
            }
            return new CodeAssembly(Assembly.LoadFile(dllPath));
        }
    }
}
