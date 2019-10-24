using System;
using System.Linq;
using Kadder.Utilies;
using Atlantis.Common.CodeGeneration;
using Atlantis.Common.CodeGeneration.Descripters;
using System.Collections.Generic;

namespace Kadder
{
    public class GrpcServiceCallBuilder
    {   
        public IDictionary<Type, string> GenerateHandler(
            GrpcOptions options, GrpcClient client, ref CodeBuilder codeBuilder)
        {
            var types = RefelectionHelper.GetImplInterfaceTypes(
                typeof(IMessagingServicer), true, options.GetScanAssemblies());
            var grpcServiceDic=new Dictionary<Type, string>();

            foreach (var typeService in types)
            {
                var className = $"{typeService.Name}GrpcService";
                var classDescripter = new ClassDescripter(className,codeBuilder.Namespace)
                    .SetBaseType(typeService.Name)
                    .AddUsing("System.Threading.Tasks", typeService.Namespace)
                    .AddUsing("Kadder")
                    .SetAccess(AccessType.Public);
                grpcServiceDic.Add(typeService,$"{codeBuilder.Namespace}.{className}");
                
                var baseInterfaces = typeService.GetInterfaces();
                foreach (var method in typeService.GetMethods())
                {
                    var notGrpcMethodCount = method.CustomAttributes.Count(
                        p => p.AttributeType == typeof(NotGrpcMethodAttribute));
                    if (notGrpcMethodCount > 0)
                    {
                        continue;
                    }

                    var parameters = method.GetParameters();
                    if (parameters.Length != 1)
                    {
                        continue;
                    }

                    var requestName=parameters[0].ParameterType.Name.ToLower();
                    var responseType=GetMethodReturn(method.ReturnType);
                    var methodName=method.Name.Replace("Async","");
                    classDescripter.CreateMember(
                        new MethodDescripter(method.Name,true)
                        .SetAccess(AccessType.Public)
                        .SetReturn($"Task<{responseType.Name}>")
                        .SetParams(new ParameterDescripter(parameters[0].ParameterType.Name,requestName))
                        .AppendCode($@"var client = GrpcClientExtension.ClientDic[""{client.ID.ToString()}""];")
                        .AppendCode($@"return await client.CallAsync<{parameters[0].ParameterType.Name},{responseType.Name}>({requestName}, ""{methodName}"", ""{typeService.Name}"");"))
                        .AddUsing(responseType.Namespace)
                        .AddUsing(parameters[0].ParameterType.Namespace);
                }
                codeBuilder.CreateClass(classDescripter)
                    .AddAssemblyRefence(typeService.Assembly.Location);
            }
            codeBuilder.AddAssemblyRefence(this.GetType().Assembly.Location);
            return grpcServiceDic;
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
}
