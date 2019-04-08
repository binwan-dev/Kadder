using System;
using System.Collections.Generic;
using Grpc.Core;
using System.Threading.Tasks;
using Atlantis.Grpc.Utilies;
using Atlantis.Grpc.Simple.Server;
namespace Atlantis.Grpc
{
    public class GrpcService : IGrpcServices
    {
        private readonly IBinarySerializer _binarySerializer;
        private readonly GrpcMessageServicer _messageServicer;
        public GrpcService()
        {
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _messageServicer = new GrpcMessageServicer();
        }

        public Task<HelloMessageResult> Hello(HelloMessage request, ServerCallContext context)
        {
            request.SetTypeFullName("Atlantis.Grpc.Simple.Server.HelloMessage");
            return _messageServicer.ProcessAsync<HelloMessage, HelloMessageResult>(request, context);
        }

        public ServerServiceDefinition BindServices()
        {
            return ServerServiceDefinition.CreateBuilder()
.AddMethod(new Method<HelloMessage, HelloMessageResult>(
                        MethodType.Unary,
                        "Atlantis.Simple.Atlantis.Simple.Service",
                        "Hello",
                        new Marshaller<HelloMessage>(
                            _binarySerializer.Serialize,
                            _binarySerializer.Deserialize<HelloMessage>
                        ),
                        new Marshaller<HelloMessageResult>(
                            _binarySerializer.Serialize,
                            _binarySerializer.Deserialize<HelloMessageResult>)
                        ),
                    Hello)
.Build();

        }

    }
}

namespace Atlantis.Grpc
{
    public class MessageServicerProxy : IMessageServicerProxy
    {
        public Func<TMessage, Task<TMessageResult>> GetHandleDelegate<TMessage, TMessageResult>(TMessage message) where TMessageResult : class where TMessage : BaseMessage
        {
            if (string.Equals(message.GetTypeFullName(), "Atlantis.Grpc.Simple.Server.HelloMessage"))
            {
                return async (m) => { return (await ObjectContainer.Resolve<IPersonMessageServicer>().HelloAsync(message as Atlantis.Grpc.Simple.Server.HelloMessage)) as TMessageResult; };
            }
            return null;
        }

        public Func<TMessage, Task> GetHandleDelegate<TMessage>(TMessage message) where TMessage : BaseMessage
        {
            return null;
        }

    }
}

