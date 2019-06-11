using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Atlantis.Common.CodeGeneration;
using Atlantis.Common.CodeGeneration.Descripters;
using Grpc.Core;
using Kadder.Utilies;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf;

namespace Kadder
{
    public class GrpcServiceBuilder
    {
        private IList<string> _messages = new List<string>();

        public ClassDescripter GenerateHandlerProxy(
            Assembly[] assemblies, CodeBuilder codeBuilder = null)
        {
            if (codeBuilder == null)
            {
                codeBuilder = CodeBuilder.Default;
            }

            var types = RefelectionHelper.GetImplInterfaceTypes(
                typeof(IMessagingServicer), true, assemblies);

            var codeClass = new ClassDescripter("MessageServicerProxy", "Kadder")
                .SetAccess(AccessType.Public)
                .SetBaseType("IMessageServicerProxy")
                .AddUsing(
                    "using System;",
                    "using Kadder.Utilies;",
                    "using System.Threading.Tasks;",
                    "using Microsoft.Extensions.DependencyInjection;");

            var needResult = new StringBuilder();
            var noResult = new StringBuilder();

            foreach (var type in types)
            {
                var methods = type.GetMethods();
                foreach (var method in methods)
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length != 1) continue;
                    var requestName = GetRequestName(method);
                    if (method.ReturnType == typeof(void))
                    {
                        noResult.AppendLine(
                            $@"if(string.Equals(message.GetTypeFullName(),""{requestName}""))
                               {{
                                   return async (m)=>await {{serviceProvider.GetService<{type.Name}>().{method.Name}(message as {parameters[0].ParameterType.FullName});}} ;   
                               }}");
                    }
                    else
                    {
                        needResult.AppendLine(
                            $@"if(string.Equals(message.GetTypeFullName(),""{requestName}""))
                               {{
                                   return async (m)=>{{return (await serviceProvider.GetService<{type.Name}>().{method.Name}(message as {parameters[0].ParameterType.FullName})) as TMessageResult;}} ;   
                               }}");
                    }
                    codeBuilder.AddAssemblyRefence(parameters[0].ParameterType.Assembly.Location);
                }
                codeClass.AddUsing($"using {type.Namespace};");
                codeBuilder.AddAssemblyRefence(type.Assembly.Location);
            }

            noResult.Append("return null;");
            needResult.Append("return null;");

            codeClass.CreateMember(
                new MethodDescripter("GetHandleDelegate<TMessage,TMessageResult>")
                .SetAccess(AccessType.Public)
                .SetReturn("Func<TMessage,Task<TMessageResult>>")
                .SetCode(needResult.ToString())
                .SetParams(
                    new ParameterDescripter("TMessage", "message"),
                    new ParameterDescripter("IServiceProvider", "serviceProvider"))
                .SetTypeParameters(
                    new TypeParameterDescripter("TMessageResult", "class"),
                    new TypeParameterDescripter("TMessage", "BaseMessage")),
                new MethodDescripter("GetHandleDelegate<TMessage>")
                .SetAccess(AccessType.Public)
                .SetReturn("Func<TMessage,Task>")
                .SetCode(noResult.ToString())
                .SetParams(
                    new ParameterDescripter("TMessage", "message"),
                    new ParameterDescripter("IServiceProvider", "serviceProvider"))
                .SetTypeParameters(
                    new TypeParameterDescripter("TMessage", "BaseMessage")));

            codeBuilder
                .AddAssemblyRefence(assemblies.Select(p => p.Location).ToArray())
                .AddAssemblyRefence(Assembly.GetExecutingAssembly().Location)
                .AddAssemblyRefence(typeof(ServiceProviderServiceExtensions).Assembly.Location)
                .CreateClass(codeClass);

            return codeClass;
        }

        public ClassDescripter GenerateGrpcProxy(
            GrpcServerOptions options, CodeBuilder codeBuilder = null)
        {
            if (codeBuilder == null)
            {
                codeBuilder = CodeBuilder.Default;
            }

            var types = RefelectionHelper.GetImplInterfaceTypes(
                typeof(IMessagingServicer), true, options.GetScanAssemblies());

            var codeClass = new ClassDescripter("GrpcService", "Kadder")
                .SetAccess(AccessType.Public)
                .SetBaseType("IGrpcServices")
                .AddUsing("using Grpc.Core;")
                .AddUsing("using System.Threading.Tasks;")
                .AddUsing("using Kadder;")
                .AddUsing("using Kadder.Utilies;")
                .AddUsing("using Microsoft.Extensions.DependencyInjection;")
                .CreateFiled(
                    new FieldDescripter("_binarySerializer")
                    .SetAccess(AccessType.PrivateReadonly)
                    .SetType(typeof(IBinarySerializer)),
                    new FieldDescripter("_messageServicer")
                    .SetAccess(AccessType.PrivateReadonly)
                    .SetType(typeof(GrpcMessageServicer)))
                .CreateConstructor(
                    new ConstructorDescripter("GrpcService")
                    .SetCode("_binarySerializer=GrpcServerBuilder.ServiceProvider.GetService<IBinarySerializer>();\n_messageServicer=GrpcServerBuilder.ServiceProvider.GetService<GrpcMessageServicer>();")
                    .SetAccess(AccessType.Public));

            var bindServicesCode = new StringBuilder("return ServerServiceDefinition.CreateBuilder()\n");
            var protoServiceCode = new StringBuilder($"service {options.ServiceName} {{");
            var protoMessageCode = new StringBuilder();
            protoServiceCode.AppendLine();
            foreach (var item in types)
            {
                var baseInterfaces = item.GetInterfaces();
                foreach (var method in item.GetMethods())
                {
                    if (method.CustomAttributes.FirstOrDefault(p => p.AttributeType == typeof(NotGrpcMethodAttribute)) != null) continue;
                    var parameters = method.GetParameters();
                    if (parameters.Length != 1) continue;
                    CreateCallCode(method, parameters[0], baseInterfaces);
                    CreateGrpcCallCode(method, parameters[0]);

                    if (options.IsGeneralProtoFile)
                    {
                        CreateProtoCode(method, parameters[0]);
                    }
                }
            }
            bindServicesCode.AppendLine(".Build();");
            codeClass.CreateMember(
                new MethodDescripter("BindServices")
                .SetCode(bindServicesCode.ToString())
                .SetReturn("ServerServiceDefinition")
                .SetAccess(AccessType.Public));
            codeBuilder.AddAssemblyRefence(Assembly.GetExecutingAssembly().Location)
                    .AddAssemblyRefence(typeof(ServerServiceDefinition).Assembly.Location)
                    .AddAssemblyRefence(typeof(ServerServiceDefinition).Assembly.Location)
                    .AddAssemblyRefence(typeof(ServiceProviderServiceExtensions).Assembly.Location);

            if (options.IsGeneralProtoFile)
            {
                protoServiceCode.Append("\n}\n");
                var protoStr = new StringBuilder();
                protoStr.AppendLine(@"syntax = ""proto3"";");
                protoStr.AppendLine($@"option csharp_namespace = ""{options.NamespaceName}"";");
                protoStr.AppendLine($"package {options.PackageName};\n");
                protoStr.Append(protoServiceCode);
                protoStr.AppendLine();
                protoStr.Append(protoMessageCode);
                var fileName = $"{Environment.CurrentDirectory}/{options.NamespaceName}.proto";
                if (File.Exists(fileName)) File.Delete(fileName);
                File.WriteAllText(fileName, protoStr.ToString());
                _messages = null;
            }

            codeBuilder.CreateClass(codeClass);

            return codeClass;

            void CreateCallCode(MethodInfo method, ParameterInfo parameter, Type[] baseInterfaces)
            {
                var requestName = GetRequestName(method);
                codeClass
                    .CreateMember(
                        new MethodDescripter($"{method.Name.Replace("Async", "")}", true)
                        .AppendCode(
                            $@"request.SetTypeFullName(""{requestName}"");
                            return await _messageServicer.ProcessAsync<{parameter.ParameterType.Name},{GetMethodReturn(method.ReturnType).Name}>(request,context);")
                        .SetReturn($"Task<{GetMethodReturn(method.ReturnType).Name}>")
                        .SetParams(
                            new ParameterDescripter(parameter.ParameterType.Name, "request"),
                            new ParameterDescripter("ServerCallContext", "context"))
                        .SetAccess(AccessType.Public))
                    .AddUsing($"using {parameter.ParameterType.Namespace};")
                    .AddUsing($"using {GetMethodReturn(method.ReturnType).Namespace};");
            }

            void CreateGrpcCallCode(MethodInfo method, ParameterInfo parameter)
            {
                bindServicesCode.AppendLine($@".AddMethod(new Method<{parameter.ParameterType.Name},{GetMethodReturn(method.ReturnType).Name}>(
                                            MethodType.Unary,
                                            ""{options.NamespaceName}.{options.ServiceName}"",
                                            ""{method.Name.Replace("Async", "")}"",
                                            new Marshaller<{parameter.ParameterType.Name}>(
                                                _binarySerializer.Serialize,
                                                _binarySerializer.Deserialize<{parameter.ParameterType.Name}>
                                            ),
                                            new Marshaller<{GetMethodReturn(method.ReturnType).Name}>(
                                                _binarySerializer.Serialize,
                                                _binarySerializer.Deserialize<{GetMethodReturn(method.ReturnType).Name}>)
                                            ),
                                        {method.Name.Replace("Async", "")})");
                codeBuilder.AddAssemblyRefence(parameter.ParameterType.Assembly.Location)
                           .AddAssemblyRefence(GetMethodReturn(method.ReturnType).Assembly.Location);
            }

            void CreateProtoCode(MethodInfo method, ParameterInfo parameter)
            {
                protoServiceCode.AppendLine();
                protoServiceCode.AppendLine($"\trpc {method.Name.Replace("Async", "")}({parameter.ParameterType.Name}) returns({GetMethodReturn(method.ReturnType).Name});");
                if (!_messages.Contains(parameter.ParameterType.Name))
                {
                    protoMessageCode.AppendLine(CreateProtoMessageCode(parameter.ParameterType));
                    _messages.Add(parameter.ParameterType.Name);
                }
                if (!_messages.Contains(GetMethodReturn(method.ReturnType).Name))
                {
                    protoMessageCode.AppendLine(CreateProtoMessageCode(GetMethodReturn(method.ReturnType)));
                    _messages.Add(GetMethodReturn(method.ReturnType).Name);
                }
            }

            string CreateProtoMessageCode(Type messageType)
            {
                //    Serializer.GetProto(messageType);

                if (_messages.Contains(messageType.Name)) return "";

                var messageCode = new StringBuilder($"message {messageType.Name} {{");
                var enumCode = new StringBuilder();
                var refenceCode = new StringBuilder();
                messageCode.AppendLine();
                var properties = messageType.GetProperties().Where(p => p.DeclaringType == messageType);
                var isNeedAutoIndex = !(properties.FirstOrDefault(p => p.GetCustomAttribute(typeof(ProtoMemberAttribute)) != null) != null);
                int index = 1;
                foreach (var item in properties.OrderBy(p => p.Name, new ProtoPropertyCompare()))
                {
                    if (item.GetCustomAttribute(typeof(ProtoIgnoreAttribute)) != null) continue;
                    if (!isNeedAutoIndex)
                    {
                        var memberAttribute = item.GetCustomAttribute<ProtoMemberAttribute>();
                        if (memberAttribute == null) continue;
                        messageCode.Append($"\t{GetFiledType(item.PropertyType)} {item.Name} = {memberAttribute.Tag};\n");
                    }
                    else
                    {
                        if (item.GetCustomAttribute(typeof(ProtoIgnoreAttribute)) != null) continue;
                        messageCode.Append($"\t{GetFiledType(item.PropertyType)} {item.Name} = {index};\n");
                        index++;
                    }

                    if (!item.PropertyType.IsGenericType && string.Equals(item.PropertyType.BaseType.Name.ToLower(), "enum") && !_messages.Contains(item.PropertyType.Name))
                    {
                        GeneralEnumCode(item.PropertyType);
                        _messages.Add(item.PropertyType.Name);
                    }

                    if (item.PropertyType.IsClass && item.PropertyType.GetCustomAttribute<ProtoContractAttribute>() != null)
                    {
                        refenceCode.AppendLine(CreateProtoMessageCode(item.PropertyType));
                    }
                    if (item.PropertyType.IsArray && item.PropertyType.GetElementType().GetCustomAttribute<ProtoContractAttribute>() != null)
                    {
                        refenceCode.AppendLine(CreateProtoMessageCode(item.PropertyType.GetElementType()));
                    }
                }
                _messages.Add(messageType.Name);
                return messageCode.Append("}\n\n").Append(enumCode).Append(refenceCode).ToString();

                string GetFiledType(Type type)
                {
                    switch (type.Name.ToLower())
                    {
                        case "int": return "int32";
                        case "int32": return "int32";
                        case "int64": return "int64";
                        case "long": return "int64";
                        case "string": return "string";
                        case "datetime": return "bcl.DateTime";
                        case "bool": return "bool";
                        case "boolean": return "bool";
                        case "double": return "double";
                        case "float": return "float";
                        default:
                            if (type.Name.Contains("[]")) return $"repeated {GetFiledType(type.GetElementType())}";
                            return type.Name;
                    }
                }

                void GeneralEnumCode(Type type)
                {
                    var zeroCode = "\tZERO = 0;";
                    var enumFiledCode = new StringBuilder();
                    var hasDefault = false;
                    foreach (var item in type.GetFields())
                    {
                        if (item.Name.Equals("value__")) continue;
                        var values = (int)item.GetValue(null);
                        if (values <= 0) hasDefault = true;
                        enumFiledCode.AppendLine($"\t{item.Name} = {values};");
                    }
                    if (hasDefault) enumCode.Append($"enum {type.Name}{{\n{enumFiledCode}}}\n");
                    else enumCode.Append($"enum {type.Name}{{\n{zeroCode}\n{enumFiledCode}}}\n");
                }
            }
        }

        private static Type GetMethodReturn(Type returnType)
        {
            var genericTypes = returnType.GetGenericArguments();
            if (genericTypes.Length == 0)
            {
                return returnType;
            }
            else
            {
                return genericTypes[0];
            }
        }

        private static string GetRequestName(MethodInfo method)
        {
            var name = $"{method.DeclaringType.FullName}.{method.Name}";
            var parameters = method.GetParameters();
            if (parameters.Length == 0)
            {
                return name;
            }

            foreach (var param in parameters)
            {
                name += $"-{param.ParameterType.Name}";
            }
            return name;
        }

    }
}
