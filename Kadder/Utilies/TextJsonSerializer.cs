using System.Text.Json;

namespace Kadder.Utilies
{
    public class TextJsonSerializer : IBinarySerializer
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
