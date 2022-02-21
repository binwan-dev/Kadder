using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using GenAssembly;
using GenAssembly.Descripters;
using Grpc.Core;
using Kadder.Utils;
using Microsoft.Extensions.Logging;

namespace Kadder.Grpc.Server
{
    public class ServicerProxyGenerator
    {
        public const string ClassProviderName = "_provider";
        public const string ClassBinarySerializerName = "_binarySerializer";
        public const string ClassLoggerName = "_log";	
        public const string FakeCallTypeAttributeName = "FakeCallType";

        private readonly List<Type> _servicerTypes;
        private readonly string _packageName;

        public ServicerProxyGenerator(string packageName, List<Type> servicerTypes)
        {
            _packageName = packageName;
            _servicerTypes = servicerTypes;
        }

        public List<ClassDescripter> Generate()
        {
            var classDescripterList = new List<ClassDescripter>();

            foreach (var servicerType in _servicerTypes)
                classDescripterList.Add(generate(servicerType));

            return classDescripterList;
        }

        private ClassDescripter generate(Type servicerType)
        {
            var classDescripter = generateClass(servicerType);
            generateField(ref classDescripter);
            generateConstructor(ref classDescripter);

            var grpcMethods = servicerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var method in grpcMethods)
            {
                if (method.CustomAttributes.FirstOrDefault(p => p.AttributeType == typeof(NotGrpcMethodAttribute)) != null)
                    continue;

                classDescripter.CreateMember(generateMethod(ref classDescripter, method));
            }
            classDescripter.CreateMember(generateBindServicesMethod(ref classDescripter, servicerType));

            return classDescripter;
        }

        #region class
        private ClassDescripter generateClass(Type servicerType)
        {
            var servicerName = $"KadderServer{servicerType.Name}";
            var namespaceName = servicerType.Namespace;
            if (!string.IsNullOrWhiteSpace(_packageName))
                namespaceName = _packageName;

            var classDescripter = new ClassDescripter(servicerName, namespaceName)
                .SetBaseType(typeof(IGrpcServices).Name)
                .AddUsing(typeof(IGrpcServices).Namespace)
                .AddUsing(
                    "using Grpc.Core;",
                    "using System.Threading.Tasks;",
                    "using Kadder;",
                    "using Kadder.Utilies;",
                    "using Kadder.Utils;",
                    "using Microsoft.Extensions.DependencyInjection;",
                    "using Kadder.Messaging;");
            classDescripter.SetAccess(AccessType.Public);
            return classDescripter;
        }

        private void generateField(ref ClassDescripter classDescripter)
        {
            var binarySerializerField = new FieldDescripter(ClassBinarySerializerName)
                .SetType(typeof(IBinarySerializer));
            binarySerializerField.SetAccess(AccessType.PrivateReadonly);

            var providerField = new FieldDescripter(ClassProviderName)
                .SetType(typeof(IObjectProvider));
            providerField.SetAccess(AccessType.PrivateReadonly);

            var loggerField = new FieldDescripter(ClassLoggerName).SetType(typeof(ILogger));
            loggerField.SetAccess(AccessType.PrivateReadonly);

            classDescripter.CreateFiled(binarySerializerField, providerField, loggerField)
                .AddUsing(typeof(IBinarySerializer).Namespace)
                .AddUsing(typeof(IObjectProvider).Namespace)
				.AddUsing(typeof(ILogger).Namespace);
        }

        private void generateConstructor(ref ClassDescripter classDescripter)
        {
            var constructor = new ConstructorDescripter(classDescripter.Name);
            constructor.SetAccess(AccessType.Public);

            var providerParameter = new ParameterDescripter(typeof(IObjectProvider).Name, "provider");
            constructor.SetParams(providerParameter);

            var code = $@"
            {ClassProviderName} = provider;
            {ClassBinarySerializerName} = provider.GetObject<IBinarySerializer>();
            {ClassLoggerName} = provider.GetObject<ILogger<{classDescripter.Name}>>();";
            constructor.SetCode(code);

            classDescripter.CreateConstructor(constructor);
        }
        #endregion

        #region method
        private MethodDescripter generateMethod(ref ClassDescripter classDescripter, MethodInfo methodInfo)
        {
            var parameterType = methodInfo.ParseMethodParameter();
            var returnType = methodInfo.ParseMethodReturnParameter();
            var callType = Helper.AnalyseCallType(parameterType, returnType);

            classDescripter.AddUsing(parameterType.Namespace, returnType.Namespace, methodInfo.DeclaringType.Namespace);

            var method = new MethodDescripter("", classDescripter);
            switch (callType)
            {
                case CallType.Rpc:
                    method = generateRpcMethod(ref classDescripter, methodInfo, parameterType, returnType);
                    break;
                case CallType.ClientStreamRpc:
                    method = generateClientStreamRpcMethod(ref classDescripter, methodInfo, parameterType, returnType);
                    break;
                case CallType.ServerStreamRpc:
                    method = generateServerStreamRpcMethod(ref classDescripter, methodInfo, parameterType, returnType);
                    break;
                case CallType.DuplexStreamRpc:
                    method = generateDuplexStreamRpcMethod(ref classDescripter, methodInfo, parameterType, returnType);
                    break;
                default:
                    throw new InvalidOperationException("Invalid Method definition!");
            }

            var methodTypeAttribute = new AttributeDescripter(FakeCallTypeAttributeName, ((int)callType).ToString());
            method.Attributes.Add(methodTypeAttribute);

            return method;
        }

        private MethodDescripter generateRpcMethod(ref ClassDescripter classDescripter, MethodInfo methodInfo, Type parameterType, Type returnType)
        {
            var resultCode = Helper.GenerateAwaitResultCode(returnType);
            var requestCode = Helper.GenerateRequestCode(parameterType);
            var servicerName = methodInfo.DeclaringType.FullName;
            var methodName = methodInfo.Name;
            var returnCode = Helper.GenerateReturnCode(returnType);

            var method = generateMethodHead(ref classDescripter, methodInfo);
            method.Parameters.Add(new ParameterDescripter(parameterType.Name, "request"));
            method.Parameters.Add(new ParameterDescripter("ServerCallContext", "context"));
            method.AppendCode(genCallCode(servicerName, methodName, resultCode, requestCode, returnCode));
            method.SetReturnType($"Task<{returnType.Name}>");

            return method;
        }
		
        private MethodDescripter generateClientStreamRpcMethod(ref ClassDescripter classDescripter, MethodInfo methodInfo, Type parameterType, Type returnType)
        {
            var resultCode = Helper.GenerateAwaitResultCode(returnType);
            var servicerName = methodInfo.DeclaringType.FullName;
            var methodName = methodInfo.Name;
            var requestParameterType = parameterType.GenericTypeArguments[0];
            var returnCode = Helper.GenerateReturnCode(returnType);

            var method = generateMethodHead(ref classDescripter, methodInfo);
            method.Parameters.Add(new ParameterDescripter($"IAsyncStreamReader<{requestParameterType.Name}>", "request"));
            method.Parameters.Add(new ParameterDescripter("ServerCallContext", "context"));
            method.AppendCode($"var streamReq = new AsyncRequestStream<{requestParameterType.Name}>(request);");
            method.AppendCode(genCallCode(servicerName, methodName, resultCode, "streamReq", returnCode));
            method.SetReturnType($"Task<{returnType.Name}>");

            classDescripter.AddUsing(typeof(IAsyncStreamReader<>).Namespace);
            classDescripter.AddUsing(typeof(AsyncRequestStream<>).Namespace);
            return method;
        }

        private MethodDescripter generateServerStreamRpcMethod(ref ClassDescripter classDescripter, MethodInfo methodInfo, Type parameterType, Type returnType)
        {
            var requestCode = $"{Helper.GenerateRequestCode(parameterType)}, responseStream";
            var servicerName = methodInfo.DeclaringType.FullName;
            var responseType = returnType.GenericTypeArguments[0];
            var methodName = methodInfo.Name;

            var method = generateMethodHead(ref classDescripter, methodInfo);
            method.Parameters.Add(new ParameterDescripter(parameterType.Name, "request"));
            method.Parameters.Add(new ParameterDescripter($"IServerStreamWriter<{responseType.Name}>", "response"));
            method.Parameters.Add(new ParameterDescripter("ServerCallContext", "context"));
            method.AppendCode($"var responseStream = new AsyncResponseStream<{responseType.Name}>(response);");
            method.AppendCode(genCallCode(servicerName, methodName, string.Empty, requestCode, string.Empty));
            method.SetReturnType("Task");

            classDescripter.AddUsing(typeof(IServerStreamWriter<>).Namespace);
            classDescripter.AddUsing(typeof(AsyncRequestStream<>).Namespace);
            return method;
        }

        private MethodDescripter generateDuplexStreamRpcMethod(ref ClassDescripter classDescripter, MethodInfo methodInfo, Type parameterType, Type returnType)
        {
            var requestParameterType = parameterType.GenericTypeArguments[0];
            var servicerName = methodInfo.DeclaringType.FullName;
            var methodName = methodInfo.Name;
            var responseType = returnType.GenericTypeArguments[0];
            var requestCode = "requestStream, responseStream";

            var method = generateMethodHead(ref classDescripter, methodInfo);
            method.Parameters.Add(new ParameterDescripter($"IAsyncStreamReader<{requestParameterType.Name}>", "request"));
            method.Parameters.Add(new ParameterDescripter($"IServerStreamWriter<{responseType.Name}>", "response"));
            method.Parameters.Add(new ParameterDescripter("ServerCallContext", "context"));
            method.AppendCode($"var requestStream = new AsyncRequestStream<{requestParameterType.Name}>(request);");
            method.AppendCode($"var responseStream = new AsyncResponseStream<{responseType.Name}>(response);");
            method.AppendCode(genCallCode(servicerName, methodName, string.Empty, requestCode, string.Empty));
            method.SetReturnType("Task");

            classDescripter.AddUsing(typeof(IServerStreamWriter<>).Namespace);
            classDescripter.AddUsing(typeof(AsyncRequestStream<>).Namespace);
            return method;
        }

        private MethodDescripter generateMethodHead(ref ClassDescripter classDescripter, MethodInfo methodInfo)
        {
            var method = new MethodDescripter(methodInfo.Name, classDescripter, true);
            method.Access = AccessType.Public;
            return method;
        }

	private string genCallCode(string servicer, string method, string result, string request, string @return)
        {
            return $@"try
            {{
                using(var scope = {ClassProviderName}.CreateScope())
                {{
                    var servicer = scope.Provider.GetObject<{servicer}>() ?? throw new ArgumentNullException(""Not found servicer({servicer}) register!"");
		    {result}await servicer.{method}({request});
		    {@return}
                }}
            }}
            catch(Exception ex)
            {{
                _log.LogError(ex,$""Handler has an unknow error! Msg: {{ex.Message}}, Servicer: {servicer}, Method: {method}"");
		throw ex;
            }}";
        }

        #endregion

        #region GrpcCallMethod
        private MethodDescripter generateBindServicesMethod(ref ClassDescripter classDescripter, Type servicerType)
        {
            var bindServicesMethod = new MethodDescripter("BindServices", classDescripter);
            bindServicesMethod.Access = AccessType.Public;
            bindServicesMethod.SetReturnType(typeof(ServerServiceDefinition));

            bindServicesMethod.AppendCode(@"return ServerServiceDefinition.CreateBuilder()");
            foreach (var method in classDescripter.Methods)
            {
                if (method.Attributes.Count == 0)
                    continue;

                var fakeMethodTypeAttribute = method.Attributes.FirstOrDefault(p => p.Name == FakeCallTypeAttributeName);
                if (fakeMethodTypeAttribute == null)
                    continue;

                var callType = (CallType)int.Parse(fakeMethodTypeAttribute.Parameters[0]);
                bindServicesMethod.AppendCode(generateBindServicesCode(classDescripter, method, callType, servicerType));

                method.Attributes.Remove(fakeMethodTypeAttribute);
            }
            bindServicesMethod.AppendCode(@"                .Build();");

            return bindServicesMethod;
        }

        private string generateBindServicesCode(ClassDescripter @class, MethodDescripter method, CallType callType, Type servicerType)
        {
            var callInfo = getCallInfo(callType, method);
            callInfo.RequestType = callInfo.RequestType.Replace("IAsyncStreamReader<", "").Replace("Task<", "").Replace(">", "");
            callInfo.ResponseType = callInfo.ResponseType.Replace("IServerStreamWriter<", "").Replace("Task<", "").Replace(">", "");

            var code = new StringBuilder();
            code.Append($@"                .AddMethod(new Method<{callInfo.RequestType}, {callInfo.ResponseType}>(
                    {callInfo.MethodType},
                    ""{@class.Namespace}.{servicerType.Name}"",
                    ""{method.Name}"",
                    new Marshaller<{callInfo.RequestType}>(
                        {ClassBinarySerializerName}.Serialize,
                        {ClassBinarySerializerName}.Deserialize<{callInfo.RequestType}>
                    ),
                    new Marshaller<{callInfo.ResponseType}>(
                        {ClassBinarySerializerName}.Serialize,
                        {ClassBinarySerializerName}.Deserialize<{callInfo.ResponseType}>
                    )),
                    {method.Name})");

            // This is will be remove, It's use compatible async
            if (method.Name.EndsWith("Async"))
			{
                code.AppendLine();
                code.Append($@"                .AddMethod(new Method<{callInfo.RequestType}, {callInfo.ResponseType}>(
                    {callInfo.MethodType},
                    ""{@class.Namespace}.{servicerType.Name}"",
					""{method.Name.Replace("Async","")}"",
                    new Marshaller<{callInfo.RequestType}>(
                        {ClassBinarySerializerName}.Serialize,
                        {ClassBinarySerializerName}.Deserialize<{callInfo.RequestType}>
                    ),
                    new Marshaller<{callInfo.ResponseType}>(
                        {ClassBinarySerializerName}.Serialize,
                        {ClassBinarySerializerName}.Deserialize<{callInfo.ResponseType}>
                    )),
                    {method.Name})");
			}

            return code.ToString();
        }

        private (String MethodType, string RequestType, string ResponseType) getCallInfo(CallType callType, MethodDescripter method)
        {
            switch (callType)
            {
                case CallType.Rpc:
                    return ("MethodType.Unary", method.Parameters[0].Type, method.ReturnTypeStr);
                case CallType.ClientStreamRpc:
                    return ("MethodType.ClientStreaming", method.Parameters[0].Type, method.ReturnTypeStr);
                case CallType.ServerStreamRpc:
                    return ("MethodType.ServerStreaming", method.Parameters[0].Type, method.Parameters[1].Type);
                case CallType.DuplexStreamRpc:
                    return ("MethodType.DuplexStreaming", method.Parameters[0].Type, method.Parameters[1].Type);
                default:
                    throw new InvalidCastException("Invalid CallType");
            }
        }
        #endregion
    }
}
