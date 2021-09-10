namespace Kadder.Streaming
{
    public interface IStreamReader<T> where T : class
    {
        T Current { get; }

        void MoveNext();
    }
}
