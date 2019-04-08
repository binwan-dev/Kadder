using System.Collections.Generic;
using System.Text;
using Followme.AspNet.Core.FastCommon.Utilities;

namespace Atlantis.Grpc.Utilies
{
    public class CodeClass
    {
        private string _name;
        private readonly CodeBuilder _codeBuilder;
        private readonly string _namespace;
        private readonly IList<string> _fileds = new List<string>();
        private readonly IList<string> _properties = new List<string>();
        private readonly IList<string> _methods = new List<string>();
        private readonly IList<string> _consturctors = new List<string>();
        private readonly IList<string> _baseTypes = new List<string>();

        public CodeClass(string name, string namespaces, CodeBuilder codeBuilder, string[] baseTypes = null)
        {
            Ensure.NotNullOrWhiteSpace(name, "The class name is not to be null!");
            Ensure.NotNullOrWhiteSpace(name, "The class namespace is not to be null!");

            _name = name;
            _namespace = namespaces;
            _codeBuilder = codeBuilder;
            _baseTypes = baseTypes;
            // _refences.Add("using System.Linq;");
        }

        public string Name => _name;

        public string Namespace => _namespace;

        public CodeClass CreateConstructor(string statements, CodeParameter[] parameters = null, CodeMemberAttribute? attributes = null)
        {
            attributes = attributes.HasValue ? attributes : new CodeMemberAttribute("public");
            var constructor = $@"{attributes.ToString()}{_name}({GetParameterStr(parameters)}){{{statements}}}";
            _consturctors.Add(constructor);
            return this;
        }

        public CodeClass CreateMember(string name, string statements, string returnType = "void", CodeParameter[] parameters = null, CodeMemberAttribute? attributes = null,string consistent=null)
        {
            attributes = attributes.HasValue ? attributes : new CodeMemberAttribute("public");
            var method = $@"{attributes.ToString()}{returnType} {name}({GetParameterStr(parameters)}){(string.IsNullOrWhiteSpace(consistent)?"":$"where {consistent}")}
                            {{
                                {statements}
                            }}";
            _methods.Add(method);
            return this;
        }

        public CodeClass CreateFiled(string name, string type, CodeMemberAttribute? attributes = null)
        {
            attributes = attributes.HasValue ? attributes : new CodeMemberAttribute("private");
            var filed = $@"{attributes}{type} {name};";
            _fileds.Add(filed);
            return this;
        }

        public CodeClass CreateProperty(string name, string type, CodeMemberAttribute? attributes = null, bool hasGet = true, bool hasSet = true)
        {
            attributes = attributes.HasValue ? attributes : new CodeMemberAttribute("public");
            var property = $@"{attributes}{type} {name}{{{(hasGet ? "get;" : "")}{(hasSet ? "set;" : "")}";
            _properties.Add(property);
            return this;
        }

        public CodeClass AddRefence(params string[] refences)
        {
            _codeBuilder.AddRefence(refences);
            return this;
        }

        public override string ToString()
        {
            var classStr = new StringBuilder();
            classStr.AppendLine($"namespace {_namespace}\n{{ ");
            classStr.AppendLine($"public class {_name}{GetBaseTypeStr()}\n{{");
            foreach (var item in _fileds) classStr.AppendLine(item);
            foreach (var item in _consturctors) classStr.AppendLine($"{item}\n");
            foreach (var item in _properties) classStr.AppendLine($"{item}\n");
            foreach (var item in _methods) classStr.AppendLine($"{item}\n");
            classStr.AppendLine("}}");

            return classStr.ToString();
        }

        private string GetParameterStr(CodeParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0) return string.Empty;
            string parameterStr = string.Empty;
            foreach (var item in parameters) parameterStr += $"{item.ToString()},";
            return parameterStr.Remove(parameterStr.Length - 1);
        }

        private string GetBaseTypeStr()
        {
            if (_baseTypes == null || _baseTypes.Count == 0) return "";
            string baseTypes = ":";
            foreach (var item in _baseTypes) baseTypes += $"{item},";
            return baseTypes.Remove(baseTypes.Length - 1);
        }
    }

    public struct CodeParameter
    {
        public CodeParameter(string type, string name)
        {
            this.Type = type;
            this.Name = name;

        }
        public string Type { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Type} {Name}";
        }
    }

    public struct CodeMemberAttribute
    {
        public CodeMemberAttribute(params string[] names)
        {
            this.Names = names;

        }
        public IList<string> Names { get; set; }

        public override string ToString()
        {
            if (Names == null || Names.Count == 0) return "private";
            string nameStr = string.Empty;
            foreach (var item in Names) nameStr += item + " ";
            return nameStr;
        }
    }
}
