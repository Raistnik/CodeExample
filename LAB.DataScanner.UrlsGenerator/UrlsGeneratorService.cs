using LAB.DataScanner.Components.Services.Generators;
using LAB.DataScanner.Components.Services.MessageBroker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LAB.DataScanner.UrlsGenerator
{
    public class UrlsGeneratorService : BackgroundService
    {
        private readonly ILogger<UrlsGeneratorService> _logger;
        private readonly IRmqPublisher _publisher;
        private readonly IOptions<UrlsGeneratorAppSettings> _appOptions;
        private readonly IOptions<UrlsGeneratorBindingSettings> _bindingOptions;

        public UrlsGeneratorService(ILogger<UrlsGeneratorService> logger, 
            IRmqPublisher rmqPublisher,
            IOptions<UrlsGeneratorAppSettings> appOptions,
            IOptions<UrlsGeneratorBindingSettings> bindingOptions)
        {
            _logger = logger;
            _publisher = rmqPublisher;
            _appOptions = appOptions;
            _bindingOptions = bindingOptions;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var service = new UrlsGeneratorEngine(_logger, _publisher, _appOptions.Value, _bindingOptions.Value);
                service.Start();
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
    }
}
