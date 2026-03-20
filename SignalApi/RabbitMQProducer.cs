
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace SignalApi
{
    public sealed class RabbitMQProducer : IMessageProducer, IDisposable
    {
        private int _timeToLive;
        private readonly ILogger<RabbitMQProducer> _logger;
        private readonly ConnectionFactory _connectionFactory;
        private IConnection? _connection = null;

        public RabbitMQProducer(IConfiguration configuration, ILogger<RabbitMQProducer> logger)
        {
            _logger = logger;
            _connectionFactory = new ConnectionFactory
            {
                HostName = configuration["Signal:RabbitMQ:HostName"] ?? "",
                UserName = configuration["Signal:RabbitMQ:UserName"] ?? "",
                Password = configuration["Signal:RabbitMQ:Password"] ?? "",
                VirtualHost = configuration["Signal:RabbitMQ:VirtualHost"] ?? "",
                Port = int.Parse(configuration["Signal:RabbitMQ:Port"] ?? "5672")
            };
            _timeToLive = configuration.GetValue<int>("Signal:TTL");
        }

        public async Task PublishAsync<T>(T message) where T : class
        {
            if (_connection == null || !_connection.IsOpen)
                _connection = await _connectionFactory.CreateConnectionAsync();

            var channel = await _connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "audio", durable: true, exclusive: false, autoDelete: false,
                arguments: new Dictionary<string, object?> { { "x-message-ttl", _timeToLive } });
            var strBody = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(strBody);
            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "audio", body: body);
        }

        public void Dispose()
        {
            if (_connection != null) {
                if (_connection.IsOpen)
                {
                    _connection.CloseAsync();
                }
                _connection.Dispose();
            }
        }
    }
}
