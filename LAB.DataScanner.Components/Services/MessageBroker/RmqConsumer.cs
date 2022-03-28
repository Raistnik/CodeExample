using LAB.DataScanner.Components.Services.MessageBroker;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace LAB.DataScanner.Components
{
    public class RmqConsumer : IRmqConsumer, IDisposable
    {
        public readonly EventingBasicConsumer Consumer;
        private readonly IModel _amqpChannel;
        private readonly string _queueName;
        private string _consumerTag;
        private bool _disposed = false;

        public RmqConsumer(IModel amqpChannel, string queueName)
        {
            if(amqpChannel == null || string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException();
            }
            _amqpChannel = amqpChannel;
            _queueName = queueName;
            Consumer = new EventingBasicConsumer(_amqpChannel);
        }

        public void Ack(BasicDeliverEventArgs args)
        {
            _amqpChannel.BasicAck(args.DeliveryTag, false);
        }

        public void StartListening(EventHandler<BasicDeliverEventArgs> onReceiveHandler)
        {
            if(onReceiveHandler == null)
            {
                throw new ArgumentNullException(nameof(onReceiveHandler));
            }
            Consumer.Received += onReceiveHandler;
            _consumerTag = _amqpChannel.BasicConsume(_queueName, false, Consumer);
        }

        public void StopListening()
        {
            if(string.IsNullOrEmpty(_consumerTag))
            {
                throw new InvalidOperationException("Consumer tag is null or empty. Probably StartListening never been called");
            }
            _amqpChannel.BasicCancel(_consumerTag);
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
