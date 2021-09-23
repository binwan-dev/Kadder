namespace Kadder.Messaging
{
    public interface IMessagingContext
    { }

    public interface IMessagingContext<T> : IMessagingContext where T : class
    {
        void Request();
    }
}
