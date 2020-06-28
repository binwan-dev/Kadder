using System;
using System.Linq;
using System.Collections.Generic;
using Kadder.CodeGeneration;
using Kadder.Messaging;
using System.Reflection;
using System.Threading.Tasks;
using GenAssembly.Descripters;
using GenAssembly;

namespace Kadder
{
    public class GrpcServiceCallBuilder
    {
        public IDictionary<Type, string> GenerateHandler(
            GrpcOptions options, GrpcClient client, ref CodeBuilder codeBuilder)
        {
            var types = options.GetKServicers();
            var grpcServiceDic = new Dictionary<Type, string>();

            foreach (var typeService in types)
            {
                var className = $"{typeService.Name}GrpcService";
                var classDescripter = new ClassDescripter(className, codeBuilder.Namespace)
                    .SetBaseType(typeService.Name)
                    .AddUsing("System.Threading.Tasks", typeService.Namespace)
                    .AddUsing("Kadder")
                    .SetAccess(AccessType.Public);
                grpcServiceDic.Add(typeService, $"{codeBuilder.Namespace}.{className}");

                var baseInterfaces = typeService.GetInterfaces();
                foreach (var method in typeService.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    var notGrpcMethodCount = method.CustomAttributes.Count(
                        p => p.AttributeType == typeof(NotGrpcMethodAttribute));
                    if (notGrpcMethodCount > 0)
                    {
                        classDescripter.CreateMember(GenerateNoGrpcMethod(method, classDescripter, codeBuilder));
                        continue;
                    }

                    var parameters = RpcParameterInfo.Convert(method.GetParameters());
                    if (parameters.Count == 0)
                    {
                        var emptyMessageType = typeof(EmptyMessage);
                        parameters.Add(new RpcParameterInfo()
                        {
                            Name = emptyMessageType.Name,
                            ParameterType = emptyMessageType,
                            IsEmpty = true
                        });
                    }

                    var requestName = parameters[0].ParameterType.Name.ToLower();
                    var responseType = GetMethodReturn(method);
                    var returnTypeCode = $"new Task<{responseType.Name}>";
                    var returnCode = "return ";
                    var requestCode = requestName;
                    if (parameters[0].IsEmpty)
                    {
                        requestCode = "new EmptyMessage()";
                    }
                    if (responseType.IsEmpty)
                    {
                        returnTypeCode = $"{method.ReturnType.Name}";
                        returnCode = string.Empty;
                    }
                    var methodName = method.Name.Replace("Async", "");
                    var methodDescripter = new MethodDescripter(method.Name, true)
                        .SetAccess(AccessType.Public)
                        .SetReturn(returnTypeCode)
                        .AppendCode($@"var client = GrpcClientExtension.ClientDic[""{client.ID.ToString()}""];")
                        .AppendCode($@"{returnCode}await client.CallAsync<{parameters[0].ParameterType.Name},{responseType.Name}>({requestCode}, ""{methodName}"", ""{typeService.Name}"");");
                    if (!parameters[0].IsEmpty)
                    {
                        methodDescripter.SetParams(new ParameterDescripter(parameters[0].ParameterType.Name, requestName));
                    }
                    classDescripter.CreateMember(methodDescripter)
                        .AddUsing(responseType.Namespace).AddUsing(parameters[0].ParameterType.Namespace);
                }
                codeBuilder.CreateClass(classDescripter)
                    .AddAssemblyRefence(typeService.Assembly);
            }
            codeBuilder.AddAssemblyRefence(this.GetType().Assembly);
            return grpcServiceDic;
        }

        private static MethodDescripter GenerateNoGrpcMethod(MethodInfo method, ClassDescripter classDescripter, CodeBuilder codeBuilder)
        {
            var methodDescripter = new MethodDescripter(method.Name)
                .SetAccess(AccessType.Public)
                .SetReturn(GetReturnName(method.ReturnType))
                .AppendCode("throw new System.NotImplementedException();");
            var parameterDescripters = new List<ParameterDescripter>();
            foreach (var param in method.GetParameters())
            {
                parameterDescripters.Add(new ParameterDescripter(GetReturnName(param.ParameterType), param.Name));
                codeBuilder.AddAssemblyRefence(param.ParameterType.Assembly);
            }
            methodDescripter.SetParams(parameterDescripters.ToArray());
            
            return methodDescripter;

            string GetReturnName(Type type)
            {
                codeBuilder.AddAssemblyRefence(type.Assembly);

                if(type.IsGenericType)
                {
                    var typeName=$"{type.FullName.Split('`')[0]}<";
                    foreach(var itemType in type.GenericTypeArguments)
                    {
                        typeName+=$"{GetReturnName(itemType)},";
                    }
                    return $"{typeName.Remove(typeName.Length-1)}>";
                }
                else if(type.IsValueType||type.Name.StartsWith("String"))
                {
                    switch(type.Name)
                    {
                        case "Int16" :return "short";
                        case "Int32" :return "int";
                        case "Int64" :return "long";
                        case "UInt16" :return "ushort";
                        case "UInt32" :return "uint";
                        case "UInt64" :return "ulong";
                        case "String" :return "string";
                        case "Double":return "double";
                        case "Single":return "float";
                        case "Decimal":return "decimal";
                        case "Boolean":return "bool";
                        default:return string.Empty;
                    }
                }
                else
                {
                    return type.FullName;
                }
            }
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
    }
}
