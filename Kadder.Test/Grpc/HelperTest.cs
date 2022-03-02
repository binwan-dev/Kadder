using System.Threading.Tasks;
using Xunit;
using Kadder.Grpc;
using System.Collections.Generic;
using System.IO;

namespace Kadder.Test.Grpc
{
    public class HelperTest
    {
        [Fact]
        public void ParseMethodReturnParameter_Test()
        {
            var taskGenericReturnMethod = typeof(TestData).GetMethod("TestReturnType");
	    var type=taskGenericReturnMethod.ParseMethodReturnParameter();
        }

        internal class TestData
        {
            public Task<HelperTest> TestReturnType(int p1)
            {
                throw new System.NotImplementedException();
            }
        }

    }
}
