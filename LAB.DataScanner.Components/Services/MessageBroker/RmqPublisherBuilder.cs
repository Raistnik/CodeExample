using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;

namespace LAB.DataScanner.Components.Services.MessageBroker
{
    public class RmqPublisherBuilder : RmqBuilder<IRmqPublisher>
    {
        private ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        private string _exchangeName;

        public RmqPublisherBuilder UsingExchange(string exchange)
        {
            if(string.IsNullOrEmpty(exchange))
            {
                throw new ArgumentNullException(nameof(exchange));
            }
            _exchangeName = exchange;
            return this;
        }

        private void PreparePublisherConnection()
        {
            _connectionFactory = new ConnectionFactory();

            _connectionFactory.UserName = _userName;
            _connectionFactory.Password = _password;
            _connectionFactory.HostName = _hostName;
            _connectionFactory.Port = _port;
            _connectionFactory.VirtualHost = _virtualHost;

            try
            {
                _connection = _connectionFactory.CreateConnection();
                _channel = _connection.CreateModel();
            }
            catch (BrokerUnreachableException e)
            {
                throw new NullReferenceException(e.Message + "Connection to rabbitmq server failed.");
            }
        }

        public override IRmqPublisher Build()
        {
            PreparePublisherConnection();
            _channel.ExchangeDeclare(exchange: _exchangeName, type: "topic", durable: true, autoDelete: false);
            return new RmqPublisher(_channel, _exchangeName);
        }
    }
}
