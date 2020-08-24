using System;
using System.Text.Json;
using Kadder.Utilies;

namespace Kadder.Simple.Server
{
    public class JsonBinarySerializer : IBinarySerializer
    {
        public T Deserialize<T>(byte[] data)
        {
            return JsonSerializer.Deserialize<T>(data);
        }

        public byte[] Serialize<T>(T obj)
        {
            return JsonSerializer.SerializeToUtf8Bytes(obj);
        }
    }
}
