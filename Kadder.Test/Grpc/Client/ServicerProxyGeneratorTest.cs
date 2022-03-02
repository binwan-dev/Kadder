using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kadder.Grpc.Client;
using Xunit;

namespace Kadder.Test.Grpc.Client
{
    public class ServicerProxyGeneratorTest
    {
	[Fact]
        public void Generate_Test()
        {
            var generator = new ServicerProxyGenerator("Test",new List<Type>(){ typeof(TestData) });
            var testDataClassDescripter = generator.Generate().FirstOrDefault();
            Assert.NotNull(testDataClassDescripter);
	    
            var notGrpcTaskGenericMethod=testDataClassDescripter.Methods.FirstOrDefault(p => p.Name == "NotGrpcTaskGenericMethod");
	    Assert.NotNull(notGrpcTaskGenericMethod);
            Assert.Equal(notGrpcTaskGenericMethod.ReturnTypeStr, "System.Threading.Tasks.Task<Kadder.Test.Grpc.Client.ServicerProxyGeneratorTest>");
            Assert.Equal(notGrpcTaskGenericMethod.Parameters.Count, 3);
	    
            var taskGenericMethod = testDataClassDescripter.Methods.FirstOrDefault(p => p.Name == "TaskGenericMethod");
            Assert.NotNull(taskGenericMethod);
	    Assert.Equal(taskGenericMethod.ReturnTypeStr, "Task<ServicerProxyGeneratorTest>");
            Assert.Equal(taskGenericMethod.Parameters.Count, 1);
        }

        internal class TestData
        {
            public Task<ServicerProxyGeneratorTest> TaskGenericMethod(ServicerProxyGeneratorTest test)
            {
                throw new System.NotImplementedException();
            }

	    [NotGrpcMethod]
            public Task<ServicerProxyGeneratorTest> NotGrpcTaskGenericMethod(int p1, int p2, string p3)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
