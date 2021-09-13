using System;
using System.Reflection;
using System.Text;
using GenAssembly.Descripters;

namespace Kadder.Grpc.Server.Proxying
{
    public class MethodGenerator
    {

        public MethodDescripter Generate(MethodInfo method, ClassDescripter @class)
        {
            var proxyerMethod = new MethodDescripter(method.Name.Replace("Async", "") ?? "", @class, true);

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

        private GenerateMessagingContextResult generateMessagingContextCode(GenerateMessagingContextInfo info)
        {
            var result = new GenerateMessagingContextResult() { MethodDescripter = info.MethodDescripter };
            if (!info.MethodInfo.DeclaringType.IsAssignableFrom(typeof(GrpcServicerBase)))
                return result;

            var code = new StringBuilder();
            code.AppendLine($@"
            var {result.MessagingContextName} = new GrpcMessagingContext({info.ServerCallContextName})");

            if (info.CallType == CallType.ClientStreamRpc || info.CallType == CallType.Rpc)
            {
                result.Code = code.ToString();
                return result;
            }

            code.AppendLine($@"
            var responseStream = new AsyncResponseStream({info.ResponseWriterName});
            {result.MessagingContextName}.SetResponseStream(responseStream);");
            result.Code = code.ToString();
            return result;
        }

        private class GenerateMessagingContextInfo
        {
            public MethodDescripter MethodDescripter { get; set; }

            public MethodInfo MethodInfo { get; set; }

            public string ResponseWriterName { get; set; }

            public string ServerCallContextName { get; set; }

            public CallType CallType { get; set; }
        }

        private class GenerateMessagingContextResult
        {
            public string Code { get; set; }

            public string MessagingContextName => "messagingContext";

            public MethodDescripter MethodDescripter { get; set; }
        }
    }
}
