using System;

namespace LAB.DataScanner.Components.Services.MessageBroker
{
    public interface IRmqPublisher : IDisposable
    {
        void Publish(byte[] message, string routingKey);
        void Publish(byte[] message, string exchange, string routingKey);
        void Publish(byte[] message, string exchange, string[] routingKeys);
    }
}
