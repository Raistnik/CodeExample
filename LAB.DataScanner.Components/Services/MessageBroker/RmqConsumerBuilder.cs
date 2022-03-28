using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LAB.DataScanner.Components.Services.MessageBroker
{
    public class RmqConsumerBuilder : RmqBuilder<IRmqConsumer>
    {
        private ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        private string _queueName;
        private string _exchangeName;
        private readonly List<string> _routingKeys = new List<string>();
        private bool _useQueueAutoCreation = false;

        public RmqConsumerBuilder UsingQueue(string queueName)
        {
            if(string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }
            if (_useQueueAutoCreation)
            {
                throw new InvalidOperationException("Queue auto creation mode used");
            }
            _queueName = queueName;         
            return this;
        }

        public RmqConsumerBuilder UsingExchange(string exchange)
        {
            if (string.IsNullOrEmpty(exchange))
            {
                throw new ArgumentNullException(nameof(exchange));
            }
            _exchangeName = exchange;
            return this;
        }

        public RmqConsumerBuilder UsingRoutingKeys(string[] routingKeys)
        {
            if (routingKeys == null || !routingKeys.Any())
            {
                throw new ArgumentException($"{nameof(routingKeys)} argument is null or empty");
            }
            _routingKeys.AddRange(routingKeys);
            return this;
        }

        public RmqConsumerBuilder WithQueueAutoCreation()
        {
            _useQueueAutoCreation = true;
            return this;
        }

        public override IRmqConsumer Build()
        {
            PrepareConsumerConnection();
            if(_useQueueAutoCreation)
            {
                _queueName = _channel.QueueDeclare().QueueName;
            }
            else
            {
                _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            }
            _channel.ExchangeDeclare(exchange: _exchangeName, type: "topic", durable: true, autoDelete: false);
            foreach(var routingKey in _routingKeys)
            {
                _channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: routingKey);
            }            
            return new RmqConsumer(_channel, _queueName);
        }

        private void PrepareConsumerConnection()
        {
            _connectionFactory = new ConnectionFactory
            {
                UserName = _userName,
                Password = _password,
                HostName = _hostName,
                Port = _port,
                VirtualHost = _virtualHost
            };

            try
            {
                _connection = _connectionFactory.CreateConnection();
                _channel = _connection.CreateModel();
            }
            catch (BrokerUnreachableException)
            {
                throw new NullReferenceException("Connection to rabbitmq server failed");
            }
        }
    }
}
