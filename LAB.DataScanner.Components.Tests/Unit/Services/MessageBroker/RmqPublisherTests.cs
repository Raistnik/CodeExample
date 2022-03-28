using LAB.DataScanner.Components.Services.MessageBroker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

namespace LAB.DataScanner.Components.Tests.Unit.Services.MessageBroker
{
    [TestClass]
    public class RmqPublisherTests
    {
        static string exchange = "components.exchange";
        static string routingKey = "components";
        static IModel channelMock = Substitute.For<IModel>();
        RmqPublisher publisher = new RmqPublisher(channelMock, exchange);
        byte[] message = new byte[] { 192, 168, 0, 1 };

        [TestMethod]
        public void ShouldPublishMessageToDefaultExchange()
        {
            // Act
            publisher.Publish(message, routingKey);

            // Assert
            channelMock.Received().BasicPublish(exchange, routingKey, false, null, message);
        }

        [TestMethod]
        public void ShouldPublishMessageWithRoutingKey()
        {
            // Act
            publisher.Publish(message, routingKey);

            // Assert
            channelMock.Received().BasicPublish(exchange, routingKey, false, null, message);
        }

        [TestMethod]
        public void ShouldPublishMessageToCertainExchangeAndRoutingKey()
        {
            // Act
            publisher.Publish(message, exchange, routingKey);

            // Assert
            channelMock.Received().BasicPublish(exchange, routingKey, false, null, message);
        }
    }
}
