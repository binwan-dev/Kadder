using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Atlantis.Grpc.Simple.Client
{
    public class ClientAgent<T> : ClientBase<T>
    {
        protected ClientAgent()
        {
        }

        protected ClientAgent(string endpointConfigurationName) : base(endpointConfigurationName)
        {
        }

        protected ClientAgent(string endpointConfigurationName, EndpointAddress remoteAddress) : base(endpointConfigurationName, remoteAddress)
        {
        }

        protected ClientAgent(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
        {
        }

        protected ClientAgent(string endpointConfigurationName, string remoteAddress) : base(endpointConfigurationName, remoteAddress)
        {
        }
    }
}
