namespace Kadder.Utilies
{
    public interface IBinarySerializer
    {
         byte[] Serialize<T>(T obj);

         T Deserialize<T>(byte[] data);
    }
}
