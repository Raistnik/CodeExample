using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using LAB.DataScanner.Components.Services.MessageBroker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;

[assembly: InternalsVisibleTo("LAB.DataScanner.Components.Tests")]
namespace LAB.DataScanner.Components.Services.Generators
{
    public class UrlsGeneratorEngine
    {
        private readonly UrlsGeneratorAppSettings _appSettings;
        private readonly UrlsGeneratorBindingSettings _bindingSettings;
        private readonly IRmqPublisher _rmqPublisher;
        private readonly ILogger _logger;

        public UrlsGeneratorEngine(ILogger logger,
            IRmqPublisher rmqPublisher, 
            UrlsGeneratorAppSettings applicationSection,
            UrlsGeneratorBindingSettings bindingSection)
        {
            _logger = logger;
            _appSettings = applicationSection;
            _bindingSettings = bindingSection;
            _rmqPublisher = rmqPublisher;
        }

        public void Start()
        {
            var jsonSettings = new JsonSerializerSettings
            {
                Error = delegate (object sender, ErrorEventArgs args)
                {
                    _logger.LogError(args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                }
            };
            var routingKeys = JsonConvert.DeserializeObject<string[]>(_bindingSettings.SenderRoutingKeys,
                jsonSettings);
            if (routingKeys == null || routingKeys.Length == 0)
            {
                throw new ArgumentException("Routing keys are invalid");
            }

            var sequences = JsonConvert.DeserializeObject<string[]>(_appSettings.Sequences, jsonSettings);
            var urlsList = BuildUrlsList(_appSettings.UrlTemplate, BuildRangeOptionsList(sequences));

            _logger.LogInformation(String.Join($",{Environment.NewLine}", urlsList));
            Publish(urlsList, _bindingSettings.SenderExchange, routingKeys);
        }

        /// <summary>
        /// Builds list of URLs containing options in defined placeholders.
        /// </summary>
        /// <param name="templateUrl">URL with defined placeholders</param>
        /// <param name="rangeOptions">list of integer arrays with options sequences</param>
        /// <returns>list of URLs</returns>
        internal IEnumerable<string> BuildUrlsList(string templateUrl, IEnumerable<IEnumerable<int>> rangeOptions)
        {
            if(string.IsNullOrEmpty(templateUrl) || rangeOptions == null || !rangeOptions.Any())
            {
                throw new ArgumentException("One of arguments is null or empty");
            }
            var urlsList = new List<string>();
            var rx = new Regex(@"\{[0-9]\}", RegexOptions.Compiled);           
            var matches = rx.Matches(templateUrl);

            if(matches.Count == 0)
            {
                throw new ArgumentException("Template URL doesn't contain placeholders");
            }
            if (matches.Count != rangeOptions.ToList().Count)
            {
                throw new ArgumentException("Number of placeholders in URL template doesn't match number of range options");
            }

            IEnumerable<IEnumerable<int>> emptyProduct = new[] { Enumerable.Empty<int>() };
            var cartesianProduct = rangeOptions.Aggregate(emptyProduct,
                (accumulatorSequence, rangeOptions) =>
                from element in accumulatorSequence
                from range in rangeOptions
                select element.Concat(new[] { range }));

            foreach(var sequence in cartesianProduct)
            {
                var tempString = templateUrl;
                foreach(var element in sequence)
                {
                    tempString = rx.Replace(tempString, element.ToString(), 1);
                }
                urlsList.Add(tempString);
            }
           
            return urlsList;
        }

        /// <summary>
        /// Builds integer arrays containing a range of numbers for every option string.
        /// </summary>
        /// <param name="optionStrings">array of option strings in a {1..10} format</param>
        /// <returns>list of integer arrays, one array per option string containing a range</returns>
        internal IEnumerable<IEnumerable<int>> BuildRangeOptionsList(string[] optionStrings)
        {
            if(optionStrings == null || optionStrings.Length == 0)
            {
                throw new ArgumentException("Options array is null or empty", nameof(optionStrings));
            }
            var rangeOptionsList = new List<IEnumerable<int>>();
            var rx = new Regex(@"\d+", RegexOptions.Compiled);
            foreach(var optionString in optionStrings)
            {
                var matches = rx.Matches(optionString);
                if(matches.Count == 2)
                {
                    int leftEndpoint = int.Parse(matches[0].Value);
                    int rightEndpoint = int.Parse(matches[1].Value);
                    if(leftEndpoint < rightEndpoint)
                    {
                        rangeOptionsList.Add(Enumerable.Range(leftEndpoint, rightEndpoint - leftEndpoint + 1));
                    }
                    else
                    {
                        throw new ArgumentException($"Range option string: {optionString} has its left endpoint greater than right endpoint");
                    }    
                }
                else
                {
                    throw new ArgumentException($"Range option string {optionString} is invalid");
                }
            }
            return rangeOptionsList;
        }

        internal void Publish(IEnumerable<string> urlsList, string exchangeName, string[] routingKeys)
        {
            var serialized = JsonConvert.SerializeObject(urlsList);
            _rmqPublisher.Publish(Encoding.UTF8.GetBytes(serialized), exchangeName, routingKeys);
        }
    }
}
