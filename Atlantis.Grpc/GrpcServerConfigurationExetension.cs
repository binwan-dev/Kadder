using Followme.AspNet.Core.FastCommon.Infrastructure;
using Followme.AspNet.Core.FastCommon.Utilities;
using System.Reflection;
using Followme.AspNet.Core.FastCommon.ThirdParty.GrpcServer;
using Grpc.Core;
using Followme.AspNet.Core.FastCommon.CodeGeneration;
using System;
using ProtoBuf;
using System.IO;
using System.Collections.Generic;
using Followme.AspNet.Core.FastCommon.Components;
using FM.ConsulInterop;
using System.Threading.Tasks;
using Followme.AspNet.Core.FastCommon.ThirdParty.GrpcServer.Middlewares;
using System.Text;
using System.Linq;

namespace Followme.AspNet.Core.FastCommon.Configurations
{
    public static class GrpcServerConfigurationExetension
    {
        private static IList<string> _messages = new List<string>();
        private static ConsulSetting _consulSetting;

        public static Configuration RegisterGrpcServer(this Configuration configuration, string namespaceName,string packageName, string serviceName, string settingConfigName, bool isGeneralProtoFile = true)
        {
            configuration.Setting.SetGrpcServerSetting(configuration.GetSetting<GrpcServerSetting>(settingConfigName) ?? throw new ArgumentNullException("Please config the 'GrpcServer' section! "));
            ObjectContainer.RegisterInstance<Server>(new Server());
            ObjectContainer.Register<GrpcHandlerBuilder>();
            ObjectContainer.Register<IGrpcLoggerFactory, GrpcLoggerFactory>();
            RegisterBussinessCode(configuration, namespaceName, packageName, serviceName, isGeneralProtoFile);

            var serviceRegister = new ServiceRegister(GetConsulSetting(configuration).Local);
            ObjectContainer.RegisterInstance<ServiceRegister>(serviceRegister);
            return configuration;
        }

        public static Configuration StartGrpcServer(this Configuration configuration)
        {
            GrpcHandlerDirector.ConfigActor();

            var setting = configuration.Setting.GetGrpcServerSetting();
            var server = ObjectContainer.Resolve<Server>();
             if (server.Ports.Count() == 0)
            {
                server.Ports.Add(new ServerPort(setting.Host, setting.Port, ServerCredentials.Insecure));
            }
            if (server.Services.Count() == 0)
            {
                server.Services.Add(ObjectContainer.Resolve<IGrpcServices>().BindServices());
            }
            server.Start();
            
            _messages = null;

            ObjectContainer.Resolve<ServiceRegister>().Register();
            return configuration;
        }

        public static async Task ShutdownGrpcServerAsync(this Configuration configuration)
        {
            var server = ObjectContainer.Resolve<Server>();
            await server.ShutdownAsync();
            await ObjectContainer.Resolve<ServiceRegister>().Deregister();
        }

        public static ConsulSetting GetConsulSetting(this Configuration configuration)
        {
            if (_consulSetting != null) return _consulSetting;

            _consulSetting = configuration.GetSetting<ConsulSetting>("Consul");
            if (_consulSetting == null) throw new ArgumentNullException("The consul config has error!");
            if (_consulSetting.Local == null) throw new ArgumentNullException("The consul local config has error!");

            return _consulSetting;
        }

        private static void RegisterBussinessCode(Configuration configuration, string namespaceName,string packageName, string serviceName, bool isGeneralProtoFile)
        {
            var types = RefelectionHelper.GetImplInterfaceTypes(typeof(IMessagingServicer), true, configuration.Setting.BussinessAssemblies);
            var queryServicerDeclareList = new List<string>();

            var codeClass = CodeBuilder.Instance.CreateClass("GrpcService", new string[] { "IGrpcServices" }, "Followme.AspNet.Core.FastCommon.ThirdParty.GrpcServer")
                .AddRefence("using Grpc.Core;")
                .AddRefence("using System.Threading.Tasks;")
                .AddRefence("using Followme.AspNet.Core.FastCommon.Serializing;")
                .AddRefence("using Followme.AspNet.Core.FastCommon.Infrastructure;")
                .CreateFiled("_binarySerializer", "IBinarySerializer", new CodeMemberAttribute("private", "readonly"))
                .CreateFiled("_messageServicer", "GrpcMessageServicer", new CodeMemberAttribute("private", "readonly"))
                .CreateConstructor("_binarySerializer=ObjectContainer.Resolve<IBinarySerializer>();\n_messageServicer=new GrpcMessageServicer();");
            var bindServicesCode = new StringBuilder("return ServerServiceDefinition.CreateBuilder()\n");
            var protoServiceCode = new StringBuilder($"service {serviceName} {{");
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

                    if (isGeneralProtoFile) CreateProtoCode(method, parameters[0]);
                }
            }
            bindServicesCode.AppendLine(".Build();");
            codeClass.CreateMember("BindServices", bindServicesCode.ToString(), "ServerServiceDefinition", null, new CodeMemberAttribute("public"));
            CodeBuilder.Instance.AddAssemblyRefence(Assembly.GetExecutingAssembly().Location)
                    .AddAssemblyRefence(typeof(ServerServiceDefinition).Assembly.Location)
                    .AddAssemblyRefence(typeof(ServerServiceDefinition).Assembly.Location);

            if (isGeneralProtoFile)
            {
                protoServiceCode.Append("\n}\n");
                var protoStr = new StringBuilder();
                protoStr.AppendLine(@"syntax = ""proto3"";");
                protoStr.AppendLine($@"option csharp_namespace = ""{namespaceName}"";");
                protoStr.AppendLine($"package {packageName};\n");
                protoStr.Append(protoServiceCode);
                protoStr.AppendLine();
                protoStr.Append(protoMessageCode);
                var fileName = $"{Environment.CurrentDirectory}/{namespaceName}.proto";
                if (File.Exists(fileName)) File.Delete(fileName);
                File.WriteAllText(fileName, protoStr.ToString());
                _messages = null;
            }
            queryServicerDeclareList.Clear();

            void CreateCallCode(MethodInfo method, ParameterInfo parameter, Type[] baseInterfaces)
            {
                string codeMessageType = "MessageExecutingType.Query";
                if (baseInterfaces.Contains(typeof(ICommandServicer))) codeMessageType = "MessageExecutingType.Command";

                codeClass.CreateMember($"{method.Name.Replace("Async","")}",
                                            $@"request.SetMessageExecutingType({codeMessageType});
                                               request.SetTypeFullName(""{parameter.ParameterType.FullName}"");
                                               return _messageServicer.ProcessAsync<{parameter.ParameterType.Name},{GetMethodReturn(method.ReturnType).Name}>(request,context);",
                                            $"Task<{GetMethodReturn(method.ReturnType).Name}>",
                                            new CodeParameter[]
                                            {
                                                 new CodeParameter(parameter.ParameterType.Name, "request"),
                                                 new CodeParameter("ServerCallContext","context")
                                            },
                                            new CodeMemberAttribute("public"))
                            .AddRefence($"using {parameter.ParameterType.Namespace};")
                         .AddRefence($"using {GetMethodReturn(method.ReturnType).Namespace};");
            }

            void CreateGrpcCallCode(MethodInfo method, ParameterInfo parameter)
            {
                bindServicesCode.AppendLine($@".AddMethod(new Method<{parameter.ParameterType.Name},{GetMethodReturn(method.ReturnType).Name}>(
                                            MethodType.Unary,
                                            ""{namespaceName}.{serviceName}"",
                                            ""{method.Name.Replace("Async","")}"",
                                            new Marshaller<{parameter.ParameterType.Name}>(
                                                _binarySerializer.Serialize,
                                                _binarySerializer.Deserialize<{parameter.ParameterType.Name}>
                                            ),
                                            new Marshaller<{GetMethodReturn(method.ReturnType).Name}>(
                                                _binarySerializer.Serialize,
                                                _binarySerializer.Deserialize<{GetMethodReturn(method.ReturnType).Name}>)
                                            ),
                                        {method.Name.Replace("Async","")})");
                CodeBuilder.Instance.AddAssemblyRefence(parameter.ParameterType.Assembly.Location)
                           .AddAssemblyRefence(GetMethodReturn(method.ReturnType).Assembly.Location);
            }

            void CreateProtoCode(MethodInfo method, ParameterInfo parameter)
            {
                protoServiceCode.AppendLine();
                protoServiceCode.AppendLine($"\trpc {method.Name.Replace("Async","")}({parameter.ParameterType.Name}) returns({GetMethodReturn(method.ReturnType).Name});");
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
                var properties = messageType.GetProperties().Where(p=>p.DeclaringType==messageType);
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
        

    }

    public static class GrpcServerConfigurationSettingExtension
    {
        private static GrpcServerSetting _setting;

        public static GrpcServerSetting GetGrpcServerSetting(this ConfigurationSetting configurationSetting)
        {
            return _setting;
        }

        public static ConfigurationSetting SetGrpcServerSetting(this ConfigurationSetting configurationSetting, GrpcServerSetting grpcServerSetting)
        {
            _setting = grpcServerSetting;
            return configurationSetting;
        }


    }

    public class ProtoPropertyCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return string.CompareOrdinal(x, y);
        }
    }
}
