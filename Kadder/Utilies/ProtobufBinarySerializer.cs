using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProtoBuf;

namespace Kadder.Utilies
{
    public class ProtobufBinarySerializer : IBinarySerializer
    {
        private readonly ILogger<ProtobufBinarySerializer> _log;

        public ProtobufBinarySerializer(ILogger<ProtobufBinarySerializer> log)
        {
            _log = log;
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
                _log.LogError(ex, $"Serialize failed! MsgName[{typeof(T).FullName}] Data[{JsonConvert.SerializeObject(data)}]");
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
                _log.LogError(ex, $"Serialize failed! MsgName[{typeof(T).FullName}] Data[{JsonConvert.SerializeObject(obj)}]");
                throw ex;
            }
        }
    }

}
