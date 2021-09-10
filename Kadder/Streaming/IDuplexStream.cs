namespace Kadder.Streaming
{
    public interface IDuplexStream<TRequest, TResponse> where TRequest : class where TResponse : class
    {
        IStream<TRequest> RequestStream { get; }

        IStream<TResponse> ResponseStream { get; }
    }
}
