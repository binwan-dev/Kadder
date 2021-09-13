using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GenAssembly;
using GenAssembly.Descripters;
using Kadder.Grpc;
using Kadder.Messaging;
using Kadder.Streaming;
using Grpc.Core;
using Kadder.Grpc.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Kadder.GrpcServer
{
    public class ServiceProxyGenerator
    {
        private readonly List<string> _messages;
        private readonly Dictionary<string, string> _oldVersionGrpcMethods = new Dictionary<string, string>();

        public ServiceProxyGenerator()
        {
            _messages = new List<string>();
        }

        public List<ClassDescripter> GenerateProxyer(List<Type> servicerTypes, string namespaceName)
        {
            var codeBuilder = new CodeBuilder(namespaceName, namespaceName);
            var classDescripterList = new List<ClassDescripter>();
            var protoMessageCode = new StringBuilder();
            var protoServiceCode = new StringBuilder();

            foreach (Type serviceType in servicerTypes)
            {
                var bindServicesCode = new StringBuilder("return ServerServiceDefinition.CreateBuilder()\n");
                protoServiceCode.AppendLine($"service {serviceType.Name} {{");
                var @class = generateProxyer(serviceType);
                var interfaces = serviceType.GetInterfaces();

                bindServicesCode.AppendLine(".Build();");
                protoServiceCode.AppendLine();
                protoServiceCode.AppendLine("}");
                @class.CreateMember(new MethodDescripter("BindServices", false).SetCode(bindServicesCode.ToString()).SetReturn("ServerServiceDefinition").SetAccess(AccessType.Public));
                codeBuilder.AddAssemblyRefence(Assembly.GetExecutingAssembly())
                    .AddAssemblyRefence(typeof(ServerServiceDefinition).Assembly)
                    .AddAssemblyRefence(typeof(ServerServiceDefinition).Assembly)
                    .AddAssemblyRefence(typeof(ServiceProviderServiceExtensions).Assembly)
                    .AddAssemblyRefence(typeof(Console).Assembly)
                    .AddAssemblyRefence(service.Assembly);
                codeBuilder.CreateClass(@class);
                classDescripterList.Add(@class);
            }
            if (options.IsGeneralProtoFile)
            {
                if (string.IsNullOrWhiteSpace(options.PackageName))
                {
                    options.PackageName = options.NamespaceName;
                }

                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("syntax = \"proto3\";");
                stringBuilder.AppendLine($@"option csharp_namespace = ""{options.NamespaceName}"";");
                stringBuilder.AppendLine($"package {options.PackageName};\n");
                stringBuilder.Append(protoServiceCode);
                stringBuilder.AppendLine();
                var messageCode = protoMessageCode.ToString();
                if (messageCode.Contains(".bcl."))
                {
                    protoMessageCode.Replace(".bcl.", "");
                    protoMessageCode.AppendLine(Bcl.Proto);
                }
                stringBuilder.Append(protoMessageCode);
                var path = $"{Environment.CurrentDirectory}/{options.NamespaceName}.proto";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                File.WriteAllText(path, stringBuilder.ToString());
                _messages = null;
            }
            return classDescripterList;
        }

        private string generateMethods(Type servicerType, ref ClassDescripter serviceProxyer)
        {
            foreach (var method in servicerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (method.CustomAttributes.FirstOrDefault(p => p.AttributeType == typeof(NotGrpcMethodAttribute)) != null)
                    continue;

                // var parameters = method.GetParameters().ToList();
                // if (parameters.Count == 0)
                // {
                //     var emptyMessageType = typeof(EmptyMessage);
                //     parameters.Add(new ParameterInfo()
                //     {
                //         Name = emptyMessageType.Name,
                //         ParameterType = emptyMessageType,
                //         IsEmpty = true
                //     });
                // }

                serviceProxyer = GenerateGrpcMethod(serviceProxyer, method, parameters[0], interfaces);
                GenerateGrpcCallCode(method, parameters[0], options.NamespaceName, codeBuilder, ref bindServicesCode);
                GenerateGrpcCallCodeForOldVersion(method, parameters[0], options.NamespaceName, options.ServiceName, codeBuilder, ref bindServicesCode);
                if (options.IsGeneralProtoFile)
                {
                    GenerateProtoCode(method, parameters[0], ref protoServiceCode, ref protoMessageCode);
                }
            }

        }

        private ClassDescripter generateProxyer(Type service)
        {
            var code =
                @"_binarySerializer = GrpcServerBuilder.ServiceProvider.GetService<IBinarySerializer>();
                 _messageServicer = GrpcServerBuilder.ServiceProvider.GetService<GrpcMessageServicer>();";
            var name = $"{service.Name}GrpcService";
            return new ClassDescripter(name, "Kadder")
                .SetAccess(AccessType.Public)
                .SetBaseType(typeof(IGrpcServices).Name)
                .AddUsing(
                    "using Grpc.Core;",
                    "using System.Threading.Tasks;",
                    "using Kadder;",
                    "using Kadder.Middlewares;",
                    "using Kadder.Utilies;",
                    "using Microsoft.Extensions.DependencyInjection;",
                    "using Kadder.Messaging;")
                .CreateFiled(
                    new FieldDescripter("_binarySerializer")
                    .SetAccess(AccessType.PrivateReadonly)
                    .SetType(typeof(IBinarySerializer)),
                    new FieldDescripter("_messageServicer")
                    .SetAccess(AccessType.PrivateReadonly)
                    .SetType(typeof(GrpcMessageServicer)))
                .CreateConstructor(new ConstructorDescripter(name).SetCode(code).SetAccess(AccessType.Public));
        }

        private Type parseMethodParameter(MethodInfo method)
        {
            var methodName = method.Name;
            var servicerName = method.DeclaringType.FullName;

            var methodParameters = method.GetParameters();
            if (methodParameters.Length > 1)
                throw new InvalidCastException($"The method({methodName}) parameter length cannot grather than 1! Servicer({servicerName})");
            if (methodParameters.Length == 0)
                return typeof(EmptyMessage);

            var parameterType = methodParameters[0].ParameterType;
            if (parameterType != typeof(IAsyncRequestStream<>) && parameterType.IsByRef)
                throw new InvalidCastException($"The method({methodName}) ParameterType invalid! Servicer({typeof(servicerName)})");

            return parameterType;
        }

        private Type parseMethodReturnParameter(MethodInfo method)
        {
            var methodName = method.Name;
            var servicerName = method.DeclaringType.FullName;
            var isVoidType = method.ReturnType == typeof(void) || (method.ReturnType == typeof(Task));

            if (method.ReturnType.IsInterface && method.ReturnType != typeof(IAsyncResponseStream<>))
                throw new InvalidCastException($"The method({methodName}) ReturnType cannot definition interface type! Servicer({servicerName})");
            if (!method.ReturnType.IsByRef && isVoidType)
                throw new InvalidCastException($"The method({methodName}) ReturnType cannot definition value type! Servicer({servicerName})");
            if (method.ReturnType != typeof(IAsyncResponseStream<>) && method.ReturnType != typeof(Task<>))
                throw new InvalidCastException($"The method({methodName}) ReturnType invalid! Servicer({servicerName})");

            if (isVoidType)
                return typeof(EmptyMessageResult);

            var genericTypes = method.ReturnType.GetGenericArguments();
            if (genericTypes.Length != 1)
                throw new InvalidCastException($"The method({methodName}) ReturnType generic argument must be one argument! Servicer({servicerName})");
            if (method.ReturnType == typeof(IAsyncResponseStream<>))
                return method.ReturnType;

            return genericTypes[0];
        }

        private string generateAwaitResultCode(Type returnType)
        {
            if (returnType == typeof(EmptyMessageResult))
                return string.Empty;
            return "var result = ";
        }

        private string generateRequestCode(Type parameterType)
        {
            if (parameterType == typeof(EmptyMessage))
                return string.Empty;
            return "request";
        }

        private string generateReturnCode(Type returnType)
        {
            if (returnType == typeof(EmptyMessageResult))
                return "new EmptyMessageResult()";
            return "result";
        }

        private



        private MethodDescripter generateRpcMethod(MethodInfo method, ClassDescripter @class, Type parameterType, Type returnType)
        {
            var resultCode = generateAwaitResultCode(returnType);
            var requestCode = generateRequestCode(parameterType);
            var servicerName = method.DeclaringType.Name;

            var proxyerMethod = new MethodDescripter(method.Name.Replace("Async", "") ?? "", @class, true);
            proxyerMethod.SetAccess(AccessType.Public);
            proxyerMethod.SetParams(
                new ParameterDescripter(parameterType.Name, "request"),
                new ParameterDescripter("ServerCallContext", "context"));
            proxyerMethod.AppendCode($@"
            {resultCode}await scope.ServiceProvider.GetService<{servicerName}>().{method.Name}({requestCode});
            return {generateReturnCode(returnType)};");
            proxyerMethod.Class.AddUsing($"using {parameterType.Namespace};");
            proxyerMethod.Class.AddUsing($"using {method.DeclaringType.Namespace};");
            proxyerMethod.Class.AddUsing($"using {returnType.Namespace};");
            return proxyerMethod;
        }

        private MethodDescripter generateClientStreamRpcMethod(MethodInfo method, ClassDescripter @class, Type parameterType, Type returnType)
        {
            var resultCode = generateAwaitResultCode(returnType);
            var servicerName = method.DeclaringType.Name;
            var requestParameterType = parameterType.GenericTypeArguments[0];

            var proxyerMethod = new MethodDescripter(method.Name.Replace("Async", "") ?? "", @class, true);
            proxyerMethod.SetAccess(AccessType.Public);
            proxyerMethod.SetParams(
                new ParameterDescripter($"IAsyncStreamReader<{requestParameterType.Name}>", "request"),
                new ParameterDescripter("ServerCallContext", "context"));
            proxyerMethod.AppendCode($@"
            var streamRequest = new AsyncRequestStream(request); 
            {resultCode}await scope.ServiceProvider.GetService<{servicerName}>().{method.Name}(streamRequest);
            return {generateReturnCode(returnType)};");
            proxyerMethod.Class.AddUsing($"using {parameterType.Namespace};");
            proxyerMethod.Class.AddUsing($"using {method.DeclaringType.Namespace};");
            proxyerMethod.Class.AddUsing($"using {returnType.Namespace};");
            proxyerMethod.Class.AddUsing($"using {typeof(IAsyncStreamReader<>).Namespace};");
            return proxyerMethod;
        }

        private MethodDescripter generateServerStreamRpcMethod(MethodInfo method, ClassDescripter @class, Type parameterType, Type returnType)
        {
            var requestCode = generateRequestCode(parameterType);
            var servicerName = method.DeclaringType.Name;
            var responseType = returnType.GenericTypeArguments[0];

            var proxyerMethod = new MethodDescripter(method.Name.Replace("Async", "") ?? "", @class, true);
            proxyerMethod.SetAccess(AccessType.Public);
            proxyerMethod.SetParams(
                new ParameterDescripter(parameterType.Name, "request"),
                new ParameterDescripter($"IAsyncStreamWriter<{responseType.Name}>", "responseStream"),
                new ParameterDescripter("ServerCallContext", "context"));
            proxyerMethod.AppendCode($@" 
            
            await scope.ServiceProvider.GetService<{servicerName}>().{method.Name}({requestCode});");
            proxyerMethod.Class.AddUsing($"using {parameterType.Namespace};");
            proxyerMethod.Class.AddUsing($"using {method.DeclaringType.Namespace};");
            proxyerMethod.Class.AddUsing($"using {returnType.Namespace};");
            proxyerMethod.Class.AddUsing($"using {typeof(IAsyncStreamWriter<>).Namespace};");
            return proxyerMethod;
        }

        private ClassDescripter generateMethod(MethodInfo method, ClassDescripter @class, Type[] baseInterfaces)
        {
            var parameterType = parseMethodParameter(method);
            var returnType = parseMethodReturnParameter(method);

            switch (Helper.AnalyseCallType(parameterType, returnType))
            {
                case CallType.Rpc:
                    @class.CreateMember(generateRpcMethod(method, @class, parameterType, returnType));
                    break;
                case CallType.ClientStreamRpc:

            }
        }

        private void GenerateGrpcCallCode(MethodInfo method, RpcParameterInfo parameter, string namespaceName,
                                          CodeBuilder codeBuilder, ref StringBuilder bindServicesCode)
        {
            bindServicesCode.AppendLine($@"
                .AddMethod(new Method<{parameter.ParameterType.Name}, {GrpcServiceBuilder.GetMethodReturn(method).Name}>(
                    MethodType.Unary,
                    ""{namespaceName}.{method.DeclaringType.Name}"",
                    ""{method.Name.Replace("Async", "")}"",
                    new Marshaller<{parameter.ParameterType.Name}>(
                        _binarySerializer.Serialize,
                        _binarySerializer.Deserialize<{parameter.ParameterType.Name}>
                    ),
                    new Marshaller<{GrpcServiceBuilder.GetMethodReturn(method).Name}>(
                        _binarySerializer.Serialize,
                        _binarySerializer.Deserialize<{GrpcServiceBuilder.GetMethodReturn(method).Name}>)
                    ),
                    {method.Name.Replace("Async", "")})");

            codeBuilder
                .AddAssemblyRefence(parameter.ParameterType.Assembly)
                .AddAssemblyRefence(GrpcServiceBuilder.GetMethodReturn(method).Assembly);
        }

        /// <summary>
        /// Support version 0.0.6 before
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameter"></param>
        /// <param name="namespaceName"></param>
        /// <param name="codeBuilder"></param>
        /// <param name="bindServicesCode"></param>
        private void GenerateGrpcCallCodeForOldVersion(MethodInfo method, RpcParameterInfo parameter, string namespaceName,
            string serviceName, CodeBuilder codeBuilder, ref StringBuilder bindServicesCode)
        {
            var serviceDefName = $"{namespaceName}.{serviceName}";
            var methodDefName = method.Name.Replace("Async", "");
            var key = $"{serviceDefName}.{methodDefName}";
            if (_oldVersionGrpcMethods.ContainsKey(key))
            {
                return;
            }
            bindServicesCode.AppendLine($@"
                .AddMethod(new Method<{parameter.ParameterType.Name}, {GrpcServiceBuilder.GetMethodReturn(method).Name}>(
                    MethodType.Unary,
                    ""{serviceDefName}"",
                    ""{methodDefName}"",
                    new Marshaller<{parameter.ParameterType.Name}>(
                        _binarySerializer.Serialize,
                        _binarySerializer.Deserialize<{parameter.ParameterType.Name}>
                    ),
                    new Marshaller<{GrpcServiceBuilder.GetMethodReturn(method).Name}>(
                        _binarySerializer.Serialize,
                        _binarySerializer.Deserialize<{GrpcServiceBuilder.GetMethodReturn(method).Name}>)
                    ),
                    {method.Name.Replace("Async", "")})");
            _oldVersionGrpcMethods.Add(key, key);
        }

        private void GenerateProtoCode(MethodInfo method, RpcParameterInfo parameter, ref StringBuilder protoServiceCode, ref StringBuilder protoMessageCode)
        {
            var str = $"rpc {method.Name.Replace("Async", "")}({parameter.ParameterType.Name}) returns({GrpcServiceBuilder.GetMethodReturn(method).Name});";
            protoServiceCode.AppendLine();
            protoServiceCode.AppendLine(str);
            if (!_messages.Contains(parameter.ParameterType.Name))
            {
                protoMessageCode.AppendLine(GenerateProtoMessageCode(parameter.ParameterType));
                _messages.Add(parameter.ParameterType.Name);
            }
            if (_messages.Contains(GrpcServiceBuilder.GetMethodReturn(method).Name))
            {
                return;
            }
            protoMessageCode.AppendLine(GenerateProtoMessageCode(GrpcServiceBuilder.GetMethodReturn(method).ReturnType));
            _messages.Add(GrpcServiceBuilder.GetMethodReturn(method).Name);
        }

        private string GenerateProtoMessageCode(Type messageType)
        {
            if (this._messages.Contains(messageType.Name))
            {
                return string.Empty;
            }
            var p = GetProto(messageType);
            _messages.AddRange(p.Types);
            return p.Proto;
        }

        private static RpcMethodReturnType GetMethodReturn(MethodInfo method)
        {
            var genericTypes = method.ReturnType.GetGenericArguments();
            if (genericTypes.Length != 0)
            {
                return new RpcMethodReturnType(genericTypes[0]);
            }

            if (method.ReturnType.FullName == typeof(Task).FullName || method.ReturnType.FullName == typeof(void).FullName)
            {
                return new RpcMethodReturnType(typeof(EmptyMessageResult), true);
            }

            return new RpcMethodReturnType(method.ReturnType);
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

        private (string Proto, List<string> Types) GetProto(Type type)
        {
            var p = RuntimeTypeModel.Default.GetSchema(type, ProtoSyntax.Proto3).Replace("\r", "");
            var arr = p.Split('\n');
            var proto = new StringBuilder();
            var types = new List<string>();
            var currentType = string.Empty;
            var isEnum = false;
            var isContent = false;
            for (var i = 0; i < arr.Length; i++)
            {
                var item = arr[i];
                if (item.StartsWith("syntax ") || item.StartsWith("package ") || item.StartsWith("import "))
                {
                    continue;
                }
                if (item.StartsWith("message"))
                {
                    currentType = item.Replace("message", "").Replace("{", "").Replace(" ", "");
                    isContent = true;
                }
                if (item.StartsWith("enum"))
                {
                    currentType = item.Replace("enum", "").Replace("{", "").Replace(" ", "");
                    isEnum = true;
                    isContent = true;
                }
                if (isContent && _messages.Contains(currentType))
                {
                    continue;
                }
                if (item.EndsWith("}"))
                {
                    isContent = false;
                    isEnum = false;
                }
                if (isEnum && !item.Contains("{"))
                {
                    var key = item.Replace(" ", "").Split('=')[0];
                    item = item.Replace(key, $"{currentType}_{key}");
                }
                types.Add(currentType);
                proto.AppendLine(item);
            }
            return (proto.ToString(), types);
        }
    }
}
