using System;
using System.IO;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Kadder.Utilies
{
    public class ProtobufBinarySerializer : IBinarySerializer
    {
        private ILogger<ProtobufBinarySerializer> _log;

        public ILogger<ProtobufBinarySerializer> Log
        {
            get
            {
                if (_log == null)
                {
                    _log = GrpcClientBuilder.ServiceProvider.GetService<ILogger<ProtobufBinarySerializer>>();
                }
                return _log;
            }
        }

        public T Deserialize<T>(byte[] data)
        {
            try
            {
                using (var memoryStream = new MemoryStream(data))
                {
                    return Serializer.Deserialize<T>(memoryStream);
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"Serialize failed! MsgName[{typeof(T).FullName}] Data[{JsonSerializer.Serialize(data)}]");
                throw ex;
            }
        }

        public byte[] Serialize<T>(T obj)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    Serializer.Serialize(memoryStream, obj);
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"Serialize failed! MsgName[{typeof(T).FullName}] Data[{JsonSerializer.Serialize(obj)}]");
                throw ex;
            }
        }
    }

}
