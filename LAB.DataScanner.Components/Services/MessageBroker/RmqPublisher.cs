using RabbitMQ.Client;
using System;

namespace LAB.DataScanner.Components.Services.MessageBroker
{
    public class RmqPublisher : IRmqPublisher
    {
        private readonly IModel _amqpChannel;
        private bool _disposed = false;
        private readonly string _exchange;

        public RmqPublisher(IModel amqpChannel, string exchange)
        {
            if(amqpChannel == null || exchange == null)
            {
                throw new ArgumentNullException();
            }
            _amqpChannel = amqpChannel;
            _exchange = exchange;
        }

        public void Publish(byte[] message, string routingKey)
        {
            if(string.IsNullOrEmpty(routingKey))
            {
                throw new ArgumentNullException(nameof(routingKey));
            }
            _amqpChannel.BasicPublish(exchange: _exchange, routingKey: routingKey, body: message);
        }

        public void Publish(byte[] message, string exchange, string routingKey)
        {
            if(string.IsNullOrEmpty(exchange) || string.IsNullOrEmpty(routingKey))
            {
                throw new ArgumentNullException();
            }
            _amqpChannel.BasicPublish(exchange: exchange, routingKey: routingKey, body: message);
        }

        public void Publish(byte[] message, string exchange, string[] routingKeys)
        {
            if (string.IsNullOrEmpty(exchange))
            {
                throw new ArgumentNullException(nameof(exchange));
            }
            for(int i = 0; i < routingKeys.Length; i++)
            {
                if(string.IsNullOrEmpty(routingKeys[i]))
                {
                    throw new ArgumentNullException($"Routing key with index {i} is null or empty");
                }
                _amqpChannel.BasicPublish(exchange: exchange, routingKey: routingKeys[i], body: message);
            }           
        }

        public void Dispose()
        {
            DisposeChannel();
            GC.SuppressFinalize(this);
        }

        protected virtual void DisposeChannel()
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                _amqpChannel.Close();
                _disposed = true;
            }
        }
    }
}
