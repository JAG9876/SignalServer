namespace SignalApi
{
    public interface IMessageProducer
    {
        Task PublishAsync<T>(T message) where T : class;
    }
}
