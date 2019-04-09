using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Atlantis.Grpc.Utilies;
using Atlantis.Common.CodeGeneration;
using Atlantis.Common.CodeGeneration.Descripters;

namespace Atlantis.Grpc
{
    public class GrpcClientBuilder
    {
        public static GrpcClientBuilder Instance = new GrpcClientBuilder();

        private GrpcClientBuilder()
        { }

        public CodeBuilder GenerateHandler(
            GrpcOptions options,CodeBuilder codeBuilder=null)
        {
            if(codeBuilder==null)
            {
                codeBuilder=CodeBuilder.Default;
            }
            var types = RefelectionHelper.GetImplInterfaceTypes(
                typeof(IMessagingServicer), true, options.ScanAssemblies);

            foreach (var typeService in types)
            {
                var className=typeService.Name.Remove(0, 1);
                var classDescripter = new ClassDescripter(
                        className,
                        codeBuilder.Namespace)
                    .SetBaseType(typeService.Name)
                    .AddUsing("System.Threading.Tasks", typeService.Namespace)
                    .AddUsing("Atlantis.Grpc")
                    .SetAccess(AccessType.Public);
                
                var baseInterfaces = typeService.GetInterfaces();
                foreach (var method in typeService.GetMethods())
                {
                    var notGrpcMethodCount = method.CustomAttributes
                        .Count(p =>
                            p.AttributeType == typeof(NotGrpcMethodAttribute));
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
                        .SetParams(
                            new ParameterDescripter(
                                parameters[0].ParameterType.Name,requestName))
                        .AppendCode($@"var client=GrpcClientInvokerExtension.TypeDic[typeof({typeService.Name})];")
                        .AppendCode($@"return await client.CallAsync<{parameters[0].ParameterType.Name},{responseType.Name}>({requestName}, ""{methodName}"");"))
                        .AddUsing(responseType.Namespace)
                        .AddUsing(parameters[0].ParameterType.Namespace);
                }
                codeBuilder.CreateClass(classDescripter)
                    .AddAssemblyRefence(typeService.Assembly.Location);
            }
            return codeBuilder;
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
