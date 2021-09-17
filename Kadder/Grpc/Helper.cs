using System;
using System.Reflection;
using System.Threading.Tasks;
using Kadder.Messaging;
using Kadder.Streaming;

namespace Kadder.Grpc
{
    public static class Helper
    {
        public static CallType AnalyseCallType(Type parameterType, Type returnParameterType)
        {
            if(parameterType.IsGenericType)
                parameterType=parameterType.GetGenericTypeDefinition();
            if(returnParameterType.IsGenericType)
                returnParameterType=returnParameterType.GetGenericTypeDefinition();
            
            if (parameterType == typeof(IAsyncRequestStream<>) && returnParameterType == typeof(IAsyncResponseStream<>))
                return CallType.DuplexStreamRpc;
            else if (parameterType == typeof(IAsyncRequestStream<>))
                return CallType.ClientStreamRpc;
            else if (returnParameterType == typeof(IAsyncResponseStream<>))
                return CallType.ServerStreamRpc;
            else
                return CallType.Rpc;
        }

        public static Type ParseMethodParameter(this MethodInfo method)
        {
            var methodName = method.Name;
            var servicerName = method.DeclaringType.FullName;

            var methodParameters = method.GetParameters();
            if (methodParameters.Length == 0)
                return typeof(EmptyMessage);

            var parameterType = methodParameters[0].ParameterType;
            if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() != typeof(IAsyncRequestStream<>) && parameterType.IsByRef)
                throw new InvalidCastException($"The method({methodName}) ParameterType invalid! Servicer({servicerName})");

            return parameterType;
        }

        public static Type ParseMethodReturnParameter(this MethodInfo method)
        {
            var methodName = method.Name;
            var servicerName = method.DeclaringType.FullName;
            var isVoidType = method.ReturnType == typeof(void) || (method.ReturnType == typeof(Task));
            var parameters = method.GetParameters();

            if (isVoidType && parameters.Length < 2)
                return typeof(EmptyMessageResult);

            var responseParameter = method.ReturnType;
            if (parameters.Length > 1)
                responseParameter = parameters[1].ParameterType;

            if (isVoidType && responseParameter.GetGenericTypeDefinition() == typeof(IAsyncResponseStream<>))
                return responseParameter;
            if (!isVoidType && responseParameter.GetGenericTypeDefinition() == typeof(Task<>))
            {
                return responseParameter.GenericTypeArguments[0];
            }

            throw new InvalidCastException($"The method({methodName}) ReturnType is Invalid! Servicer({servicerName})");
        }

        public static string GenerateAwaitResultCode(Type returnType)
        {
            if (returnType == typeof(EmptyMessageResult))
                return string.Empty;
            return "var result = ";
        }

        public static string GenerateRequestCode(Type parameterType)
        {
            if (parameterType == typeof(EmptyMessage))
                return string.Empty;
            return "request";
        }

        public static string GenerateReturnCode(Type returnType)
        {
            if (returnType == typeof(EmptyMessageResult))
                return "new EmptyMessageResult()";
            return "result";
        }

    }
}
