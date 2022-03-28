using LAB.DataScanner.Components.Services.Generators;
using LAB.DataScanner.Components.Services.MessageBroker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LAB.DataScanner.Components.Tests.Unit.Services.Generators
{
    [TestClass]
    public class UrlsGeneratorEngineTests
    {
        ILogger logger;
        UrlsGeneratorAppSettings optionsApp;
        UrlsGeneratorBindingSettings optionsBinding;
        IRmqPublisher publisherMock;
        UrlsGeneratorEngine urlsGenerator;
        List<string> expected;
        string serialized;
        string[] routingKeys;

        [TestInitialize]
        public void Setup()
        {
            logger = Substitute.For<ILogger>();
            optionsApp = new UrlsGeneratorAppSettings();
            optionsBinding = new UrlsGeneratorBindingSettings();
            publisherMock = Substitute.For<IRmqPublisher>();
            expected = new List<string>
            {
                "http://testSite/0",
                "http://testSite/1",
                "http://testSite/2"
            };
            serialized = JsonConvert.SerializeObject(expected);
            routingKeys = new string[] { "A", "B" };
        }

        [TestMethod]
        public void BuildRangeOptionsList_ValidInput_ValidOutput()
        {
            // Arrange
            var expectedUrlsList = new List<IEnumerable<int>>
            {
                new int[] { 0, 1, 2 },
                new int[] { 3, 4, 5, 6 },
                new int[] { 4, 5 }
            };
            var optionStrings = new string[] { "0..2", "3..6", "4..5" };
            urlsGenerator = new UrlsGeneratorEngine(logger, publisherMock, optionsApp, optionsBinding);

            // Act
            var rangeOptionsList = urlsGenerator.BuildRangeOptionsList(optionStrings).ToList();

            // Assert
            Assert.AreEqual(rangeOptionsList.Count, expectedUrlsList.Count);
            for (int i = 0; i < rangeOptionsList.Count; i++)
            {
                CollectionAssert.AreEqual(expectedUrlsList[i].ToList(), rangeOptionsList[i].ToList());
            }
        }

        [TestMethod]
        public void BuildUrlsList_ValidInput_ValidOutput()
        {
            // Arrange
            var expectedUrlsList = new List<string>
            {
                "https://example.com/blog?page=0&parameter=5&value=4",
                "https://example.com/blog?page=0&parameter=5&value=5",
                "https://example.com/blog?page=0&parameter=6&value=4",
                "https://example.com/blog?page=0&parameter=6&value=5",
                "https://example.com/blog?page=1&parameter=5&value=4",
                "https://example.com/blog?page=1&parameter=5&value=5",
                "https://example.com/blog?page=1&parameter=6&value=4",
                "https://example.com/blog?page=1&parameter=6&value=5",
                "https://example.com/blog?page=2&parameter=5&value=4",
                "https://example.com/blog?page=2&parameter=5&value=5",
                "https://example.com/blog?page=2&parameter=6&value=4",
                "https://example.com/blog?page=2&parameter=6&value=5"
            };
            var url = "https://example.com/blog?page={0}&parameter={1}&value={2}";            
            var options = new List<int[]>
            {
                new int[] { 0, 1, 2 },
                new int[] { 5, 6 },
                new int[] { 4, 5 }
            };
            urlsGenerator = new UrlsGeneratorEngine(logger, publisherMock, optionsApp, optionsBinding);

            // Act
            var urlsList = urlsGenerator.BuildUrlsList(url, options);

            // Assert
            CollectionAssert.AreEquivalent(expectedUrlsList, urlsList.ToList());
        }

        [TestMethod]
        public void ShouldGenerateAndPublishUrlsBasedOnConfiguration()
        {
            // Arrange           
            optionsApp = new UrlsGeneratorAppSettings 
                { 
                    UrlTemplate = "http://testSite/{0}", 
                    Sequences = "['0..2']" 
                };
            optionsBinding = new UrlsGeneratorBindingSettings
                { 
                    SenderExchange = "TargetExchange", 
                    SenderRoutingKeys = "['A', 'B']"
                };
            urlsGenerator = new UrlsGeneratorEngine(logger, publisherMock, optionsApp, optionsBinding);

            // Act
            urlsGenerator.Start();

            // Assert
            publisherMock.Received().Publish(
                    Arg.Is<byte[]>(e => Encoding.UTF8.GetString(e) == serialized),
                    Arg.Is("TargetExchange"),
                    Arg.Is<string[]>(e => e.SequenceEqual(routingKeys)));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldSkipPublishingIfNoAnyBindingsInfo()
        {
            // Arrange
            optionsApp = new UrlsGeneratorAppSettings
            {
                UrlTemplate = "http://testSite/{0}",
                Sequences = "['0..2']"
            };
            optionsBinding = new UrlsGeneratorBindingSettings
            {
                SenderExchange = "",
                SenderRoutingKeys = ""
            };
            urlsGenerator = new UrlsGeneratorEngine(logger, publisherMock, optionsApp, optionsBinding);

            // Act
            urlsGenerator.Start();

            // Assert
            publisherMock.Received(0).Publish(
                    Arg.Is<byte[]>(e => Encoding.UTF8.GetString(e) == serialized),
                    Arg.Is("TargetExchange"),
                    Arg.Is<string[]>(e => e.SequenceEqual(routingKeys)));
        }
    }
}
