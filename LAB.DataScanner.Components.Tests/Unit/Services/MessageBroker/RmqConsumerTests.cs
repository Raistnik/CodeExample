using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace LAB.DataScanner.Components.Tests.Unit.Services.MessageBroker
{
    [TestClass]
    public class RmqConsumerTests
    {
        static string queueName = "components.queue";
        static IModel basicConsumeMock = Substitute.For<IModel>();
        RmqConsumer consumer = new RmqConsumer(basicConsumeMock, queueName);

        void OnReceiveMock(object sender, BasicDeliverEventArgs e)
        {
            // empty event handler to mock event handler          
        }

        [TestMethod]
        public void ShouldCall_AckMessage_OnceTheyArrivedAndHandled()
        {
            // Arrange
            var eventArgs = Substitute.For<BasicDeliverEventArgs>();
            eventArgs.DeliveryTag = 65535;

            // Act
            consumer.Ack(eventArgs);

            // Assert
            basicConsumeMock.Received().BasicAck(eventArgs.DeliveryTag, false);
        }

        [TestMethod]
        public void ShouldCall_BasicConsume_OnceStartListening()
        {
            // Act
            consumer.StartListening(OnReceiveMock);

            // Assert
            basicConsumeMock.Received().BasicConsume(queueName, false, consumer.Consumer);
        }

        [TestMethod]
        public void ShouldCall_BasicCancel_OnceStopListening()
        {
            // Arrange
            var consumerTag = "a1b2bc3";
            basicConsumeMock.BasicConsume(queueName, false, consumer.Consumer).Returns(consumerTag);

            // Act
            consumer.StartListening(OnReceiveMock);
            consumer.StopListening();

            // Assert
            basicConsumeMock.Received().BasicCancel(consumerTag);
        }
    }
}
