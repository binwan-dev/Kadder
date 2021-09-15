using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GenAssembly;
using GenAssembly.Descripters;
using Grpc.Core;
using Kadder.Messaging;
using Kadder.Streaming;
using Kadder.Utilies;
using Kadder.Utils;

namespace Kadder.Grpc.Server
{
    public class ServicerProxyGenerator
    {
        public const string ClassProviderName = "_provider";
        public const string ClassBinarySerializerName = "_binarySerializer";

        public List<ClassDescripter> Generate(List<Type> servicerTypes, string namespaceName)
        {
            var codeBuilder = new CodeBuilder(namespaceName, namespaceName);
            var classDescripterList = new List<ClassDescripter>();

            foreach (var servicerType in servicerTypes)
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
            //         var @class = generateProxyer(serviceType);
            //         var interfaces = servicerType.GetInterfaces();

            //         @class.CreateMember(new MethodDescripter("BindServices", false).SetCode(bindServicesCode.ToString()).SetReturn("ServerServiceDefinition").SetAccess(AccessType.Public));
            //         codeBuilder.AddAssemblyRefence(Assembly.GetExecutingAssembly())
            //             .AddAssemblyRefence(typeof(ServerServiceDefinition).Assembly)
            //             .AddAssemblyRefence(typeof(ServerServiceDefinition).Assembly)
            //             .AddAssemblyRefence(typeof(ServiceProviderServiceExtensions).Assembly)
            //             .AddAssemblyRefence(typeof(Console).Assembly)
            //             .AddAssemblyRefence(service.Assembly);
            //         codeBuilder.CreateClass(@class);
            //         classDescripterList.Add(@class);

        }

        private ClassDescripter generateClass(Type servicerType)
        {
            var servicerName = servicerType.Name;
            if (servicerName[0] == 'I')
                servicerName = servicerName.Substring(1);
            var namespaceName = $"{servicerType.Namespace}.GrpcServicer";

            return new ClassDescripter(servicerName, namespaceName)
                .SetAccess(AccessType.Public)
                .SetBaseType(typeof(IGrpcServices).Name)
                .AddUsing(typeof(IGrpcServices).Namespace)
                .AddUsing(
                    "using Grpc.Core;",
                    "using System.Threading.Tasks;",
                    "using Kadder;",
                    "using Kadder.Middlewares;",
                    "using Kadder.Utilies;",
                    "using Microsoft.Extensions.DependencyInjection;",
                    "using Kadder.Messaging;");
        }

        private void generateField(ref ClassDescripter classDescripter)
        {
            var binarySerializerField = new FieldDescripter(ClassBinarySerializerName)
                .SetAccess(AccessType.PrivateReadonly)
                .SetType(typeof(IBinarySerializer));

            var providerField = new FieldDescripter(ClassProviderName)
                .SetAccess(AccessType.PrivateReadonly)
                .SetType(typeof(IObjectProvider));

            classDescripter.CreateFiled(binarySerializerField, providerField)
                .AddUsing(typeof(IBinarySerializer).Namespace)
                .AddUsing(typeof(IObjectProvider).Namespace);
        }

        private void generateConstructor(ref ClassDescripter classDescripter)
        {
            var constructor = new ConstructorDescripter(classDescripter.Name);
            constructor.SetAccess(AccessType.Public);

            var providerParameter = new ParameterDescripter(typeof(IObjectProvider).Name, "provider");
            constructor.SetParams(providerParameter);

            var code = $@"
            {ClassProviderName} = provider;
            {ClassBinarySerializerName} = provider.GetObject<IBinarySerializer>();";
            constructor.SetCode(code);

            classDescripter.CreateConstructor(constructor);
        }

        private MethodDescripter generateMethod(ref ClassDescripter classDescripter, MethodInfo methodInfo)
        {
            var parameterType = parseMethodParameter(methodInfo);
            var returnType = parseMethodReturnParameter(methodInfo);

            classDescripter.AddUsing(parameterType.Namespace, returnType.Namespace, methodInfo.DeclaringType.Namespace);
            
            switch (Helper.AnalyseCallType(parameterType, returnType))
            {
                case CallType.Rpc:
                    return generateRpcMethod(ref classDescripter, methodInfo, parameterType, returnType);
                case CallType.ClientStreamRpc:

            }
        }

        private MethodDescripter generateRpcMethod(ref ClassDescripter classDescripter, MethodInfo methodInfo, Type parameterType, Type returnType)
        {
            var resultCode = generateAwaitResultCode(returnType);
            var requestCode = generateRequestCode(parameterType);
            var servicerName = methodInfo.DeclaringType.Name;

            var method = generateMethodHead(ref classDescripter, methodInfo);
            method.SetParams(new ParameterDescripter(parameterType.Name, "request"));
            method.SetParams(new ParameterDescripter("ServerCallContext", "context"));

            method.AppendCode($@"
            using(var scope = {ClassProviderName}.CreateScope())
            {{
                {resultCode}await scope.Provider.GetService<{servicerName}>().{methodInfo.Name}({requestCode});
                return {generateReturnCode(returnType)};
            }}");

            return method;
        }

        private MethodDescripter generateClientStreamRpcMethod(ref ClassDescripter classDescripter, MethodInfo methodInfo, Type parameterType, Type returnType)
        {
            var resultCode = generateAwaitResultCode(returnType);
            var servicerName = methodInfo.DeclaringType.Name;
            var requestParameterType = parameterType.GenericTypeArguments[0];

            var method = generateMethodHead(ref classDescripter, methodInfo);
            method.SetParams(new ParameterDescripter($"IAsyncStreamReader<{requestParameterType.Name}>", "request"));
            method.SetParams(new ParameterDescripter("ServerCallContext", "context"));

            method.AppendCode($@"
            using(var scope = {ClassProviderName}.CreateScope())
            {{
                var streamRequest = new AsyncRequestStream(request); 
                {resultCode}await scope.ServiceProvider.GetService<{servicerName}>().{methodInfo.Name}(streamRequest);
                return {generateReturnCode(returnType)};
            }}");

            classDescripter.AddUsing(typeof(IAsyncStreamReader<>).Namespace);
            return method;
        }

        private MethodDescripter generateServerStreamRpcMethod(ref ClassDescripter classDescripter, MethodInfo methodInfo, Type parameterType, Type returnType)
        {
            var requestCode = generateRequestCode(parameterType);
            var servicerName = methodInfo.DeclaringType.Name;
            var responseType = returnType.GenericTypeArguments[0];

            var method = generateMethodHead(ref classDescripter, methodInfo);
            method.SetParams(new ParameterDescripter(parameterType.Name, "request"));
            method.SetParams(new ParameterDescripter($"IAsyncStreamWriter<{responseType.Name}>", "responseStream"));
            method.SetParams(new ParameterDescripter("ServerCallContext", "context"));
            
            method.AppendCode($@" 
            using(var scope = {ClassProviderName}.CreateScope)
            {{
                await scope.ServiceProvider.GetService<{servicerName}>().{methodInfo.Name}({requestCode});
            }}");

            classDescripter.AddUsing(typeof(IAsyncStreamWriter<>).Namespace);
            return method;
        }

        private MethodDescripter generateDuplexStreamRpcMethod(ref ClassDescripter classDescripter,MethodInfo methodInfo, Type parameterType, Type returnType)
        {
            var requestParameterType = parameterType.GenericTypeArguments[0];
            var servicerName = methodInfo.DeclaringType.Name;
            var responseType = returnType.GenericTypeArguments[0];

            var method = generateMethodHead(ref classDescripter, methodInfo);
            method.SetParams(new ParameterDescripter($"IAsyncStreamReader<{requestParameterType.Name}>", "request"));
            method.SetParams(new ParameterDescripter($"IAsyncStreamWriter<{responseType.Name}>", "response"));
            method.SetParams(new ParameterDescripter("ServerCallContext", "context"));
            
            method.AppendCode($@" 
            using(var scope = {ClassProviderName}.CreateScope)
            {{
                await scope.ServiceProvider.GetService<{servicerName}>().{methodInfo.Name}({requestCode});
            }}");

            classDescripter.AddUsing(typeof(IAsyncStreamWriter<>).Namespace);
            return method;
        }

        private MethodDescripter generateMethodHead(ref ClassDescripter classDescripter, MethodInfo methodInfo)
        {
            var methodName = methodInfo.Name.Replace("Async", "");

            return new MethodDescripter(methodName, classDescripter, true)
                .SetAccess(AccessType.Public);
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
                throw new InvalidCastException($"The method({methodName}) ParameterType invalid! Servicer({servicerName})");

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

    }
}
