namespace Kadder.Utilies
{
    public class NullJsonSerializer : IJsonSerializer
    {
        public T Deserialize<T>(string jsonStr)
        {
            throw new System.NotImplementedException();
        }

        public object Deserialize(string jsonStr)
        {
            throw new System.NotImplementedException();
        }

        public string Serialize(object obj)
        {
            throw new System.NotImplementedException();
        }
    }
}
