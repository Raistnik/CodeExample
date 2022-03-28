using RabbitMQ.Client.Events;
using System;

namespace LAB.DataScanner.Components.Services.MessageBroker
{
    public interface IRmqConsumer : IDisposable
    {
        void Ack(BasicDeliverEventArgs args);
        void StartListening(EventHandler<BasicDeliverEventArgs> onReceiveHandler);
        void StopListening();
    }
}
