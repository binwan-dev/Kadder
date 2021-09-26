using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using GenAssembly;
using GenAssembly.Descripters;
using Grpc.Core;
using Kadder.Messaging;
using Kadder.Utils;

namespace Kadder.Grpc.Client
{
    public class ServicerProxyGenerator
    {
        public const string ClassServicerInvokerName = "_invoker";

        private readonly string _packageName;
        private readonly IList<Type> _servicerTypes;

        public ServicerProxyGenerator(string packageName, IList<Type> servicerTypes)
        {
            _packageName = packageName;
            _servicerTypes = servicerTypes;
        }

        public IList<ClassDescripter> Generate()
        {
            var proxyerDescripters = new List<ClassDescripter>();
            foreach (var servicerType in _servicerTypes)
                proxyerDescripters.Add(generate(servicerType));

            return proxyerDescripters;
        }

        private ClassDescripter generate(Type servicerType)
        {
            var classDescripter = generateClass(servicerType);
            generateField(ref classDescripter);
            generateConstructor(ref classDescripter);

            var grpcMethods = ServicerHelper.GetMethod(servicerType);
            foreach (var method in grpcMethods)
                classDescripter.CreateMember(generateMethod(ref classDescripter, method));
            classDescripter.CreateMember(generateBindServicesMethod(ref classDescripter));

            return classDescripter;
        }

        #region genclass
        private ClassDescripter generateClass(Type servicerType)
        {
            var servicerName = $"KadderClient{servicerType.Name}";
            var namespaceName = $"{servicerType.Namespace}";
            if (!string.IsNullOrWhiteSpace(_packageName))
                namespaceName = _packageName;

            var classDescripter = new ClassDescripter(servicerName, namespaceName)
                .SetBaseType(servicerType.Name)
                .AddUsing(servicerType.Namespace)
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
            var invokerField = new FieldDescripter(ClassServicerInvokerName)
                .SetType(typeof(ServicerInvoker));
            invokerField.SetAccess(AccessType.PrivateReadonly);

            classDescripter.CreateFiled(invokerField)
                .AddUsing(typeof(ServicerInvoker).Namespace);
        }

        private void generateConstructor(ref ClassDescripter classDescripter)
        {
            var constructor = new ConstructorDescripter(classDescripter.Name);
            constructor.SetAccess(AccessType.Public);

            var providerParameter = new ParameterDescripter(typeof(ServicerInvoker).Name, "invoker");
            constructor.SetParams(providerParameter);

            var code = $@"
            {ClassServicerInvokerName} = invoker;";
            constructor.SetCode(code);

            classDescripter.CreateConstructor(constructor);
        }
        #endregion

        #region genmethod
        private MethodDescripter generateMethod(ref ClassDescripter classDescripter, MethodInfo methodInfo)
        {
            var parameterType = methodInfo.ParseMethodParameter();
            var returnType = methodInfo.ParseMethodReturnParameter();
            var callType = Helper.AnalyseCallType(parameterType, returnType);

            classDescripter.AddUsing(parameterType.Namespace, returnType.Namespace, methodInfo.DeclaringType.Namespace);

            if (methodInfo.CustomAttributes.FirstOrDefault(p => p.AttributeType == typeof(NotGrpcMethodAttribute)) != null)
                return generateNoGrpcMethod(ref classDescripter, methodInfo, parameterType, returnType);

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

            return method;
        }

        private MethodDescripter generateRpcMethod(ref ClassDescripter classDescripter, MethodInfo methodInfo, Type parameterType, Type returnType)
        {
            var resultCode = Helper.GenerateAwaitResultCode(returnType);
            var resultType = generateRpcResponseType(returnType);
            var servicerName = methodInfo.DeclaringType.FullName;
            var methodName = methodInfo.Name.Replace("Async", "");

            var method = generateMethodHead(ref classDescripter, methodInfo);
            if (parameterType != typeof(EmptyMessage))
                method.Parameters.Add(new ParameterDescripter(parameterType.Name, "request"));
            else
                method.AppendCode("var request = new EmptyMessage();");

            method.AppendCode($@"{resultCode} await {ClassServicerInvokerName}.RpcAsync<{parameterType.Name}, {returnType.Name}>(request, ""{servicerName}"", ""{methodName}"");
            {Helper.GenerateReturnCode(returnType, true)}");
            method.SetReturnType(resultType);

            return method;
        }

        private MethodDescripter generateClientStreamRpcMethod(ref ClassDescripter classDescripter, MethodInfo methodInfo, Type parameterType, Type returnType)
        {
            var servicerName = methodInfo.DeclaringType.FullName;
            var resultCode = Helper.GenerateAwaitResultCode(returnType);
            var resultType = generateRpcResponseType(returnType);
            var methodName = methodInfo.Name.Replace("Async", "");
            var requestType = parameterType.GenericTypeArguments[0];

            var method = generateMethodHead(ref classDescripter, methodInfo, false);
            method.Parameters.Add(new ParameterDescripter($"IAsyncRequestStream<{requestType.Name}>", "request"));

            method.AppendCode($@"{resultCode}{ClassServicerInvokerName}.ClientStreamAsync<{requestType.Name}, {returnType.Name}>(request, ""{servicerName}"", ""{methodName}"");
            {Helper.GenerateReturnCode(returnType, true)}");
            method.SetReturnType(resultType);

            return method;
        }

        private MethodDescripter generateServerStreamRpcMethod(ref ClassDescripter classDescripter, MethodInfo methodInfo, Type parameterType, Type returnType)
        {
            var servicerName = methodInfo.DeclaringType.FullName;
            var requestCode = Helper.GenerateRequestCode(parameterType);
            var methodName = methodInfo.Name.Replace("Async", "");
            var responseType = returnType.GenericTypeArguments[0];

            if (!string.IsNullOrWhiteSpace(requestCode))
                requestCode += ", ";

            var method = generateMethodHead(ref classDescripter, methodInfo, false);
            method.Parameters.Add(new ParameterDescripter(parameterType.Name, "request"));
            method.Parameters.Add(new ParameterDescripter($"IAsyncResponseStream<{responseType.Name}>", "response"));

            method.AppendCode($@"return {ClassServicerInvokerName}.ServerStreamAsync<{parameterType.Name}, {responseType.Name}>({requestCode}response, ""{servicerName}"", ""{methodName}"");");
            method.SetReturnType("Task");

            return method;
        }

        private MethodDescripter generateDuplexStreamRpcMethod(ref ClassDescripter classDescripter, MethodInfo methodInfo, Type parameterType, Type returnType)
        {
            var servicerName = methodInfo.DeclaringType.FullName;
            var methodName = methodInfo.Name.Replace("Async", "");
            var requestType = parameterType.GenericTypeArguments[0];
            var responseType = returnType.GenericTypeArguments[0];

            var method = generateMethodHead(ref classDescripter, methodInfo, false);
            method.Parameters.Add(new ParameterDescripter($"IAsyncRequestStream<{requestType.Name}>", "request"));
            method.Parameters.Add(new ParameterDescripter($"IAsyncResponseStream<{responseType.Name}>", "response"));

            method.AppendCode($@"return {ClassServicerInvokerName}.DuplexStreamAsync<{requestType.Name}, {responseType.Name}>(request, response, ""{servicerName}"", ""{methodName}"");");
            method.SetReturnType("Task");

            return method;
        }

        private MethodDescripter generateNoGrpcMethod(ref ClassDescripter classDescripter, MethodInfo methodInfo, Type parameterType, Type returnType)
        {
            var method = new MethodDescripter(methodInfo.Name, classDescripter, false);
            method.Access = AccessType.Public;
            method.SetReturnType(GetReturnName(ref classDescripter, methodInfo.ReturnType));

            var parameterDescripters = new List<ParameterDescripter>();
            foreach (var param in methodInfo.GetParameters())
            {
                classDescripter.AddUsing(param.ParameterType.Namespace);
                method.Parameters.Add(new ParameterDescripter(GetReturnName(ref classDescripter, param.ParameterType), param.Name));
            }

            method.AppendCode("throw new System.NotImplementedException();");

            return method;
        }

        private MethodDescripter generateMethodHead(ref ClassDescripter classDescripter, MethodInfo methodInfo, bool isAsync = true)
        {
            var method = new MethodDescripter(methodInfo.Name, classDescripter, isAsync);
            method.Access = AccessType.Public;
            return method;
        }

        private string generateRpcResponseType(Type returnType)
        {
            if (returnType.Name == typeof(EmptyMessageResult).Name)
                return "Task";

            return $"Task<{returnType.Name}>";
        }

        private string GetReturnName(ref ClassDescripter classDescripter, Type type)
        {
            classDescripter.AddUsing(type.Namespace);

            if (type.IsGenericType)
            {
                var typeName = $"{type.FullName.Split('`')[0]}<";
                foreach (var itemType in type.GenericTypeArguments)
                {
                    typeName += $"{GetReturnName(ref classDescripter, itemType)},";
                }
                return $"{typeName.Remove(typeName.Length - 1)}>";
            }
            else if (type.IsValueType || type.Name.StartsWith("String"))
            {
                switch (type.Name)
                {
                    case "Int16": return "short";
                    case "Int32": return "int";
                    case "Int64": return "long";
                    case "UInt16": return "ushort";
                    case "UInt32": return "uint";
                    case "UInt64": return "ulong";
                    case "String": return "string";
                    case "Double": return "double";
                    case "Single": return "float";
                    case "Decimal": return "decimal";
                    case "Boolean": return "bool";
                    default: return string.Empty;
                }
            }
            else
            {
                return type.FullName;
            }
        }
        #endregion

        #region GrpcCallMethod
        private MethodDescripter generateBindServicesMethod(ref ClassDescripter classDescripter)
        {
            var bindServicesMethod = new MethodDescripter("BindServices", classDescripter);
            bindServicesMethod.Access = AccessType.Public;
            bindServicesMethod.SetReturnType(typeof(ServerServiceDefinition));
            bindServicesMethod.AppendCode(@"throw new NotImplementedException();");
            return bindServicesMethod;
        }
        #endregion
    }
}
