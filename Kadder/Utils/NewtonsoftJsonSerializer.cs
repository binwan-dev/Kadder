using Newtonsoft.Json;

namespace Kadder.Utilies
{
    public class NewtonsoftJsonSerializer:IJsonSerializer
    {
        public T Deserialize<T>(string jsonStr)
        {
            return JsonConvert.DeserializeObject<T>(jsonStr);
        }

        public object Deserialize(string jsonStr)
        {
            return JsonConvert.DeserializeObject(jsonStr);
        }

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

    }
}
