using LAB.DataScanner.Components.Services.Generators;
using LAB.DataScanner.Components.Services.MessageBroker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;

namespace LAB.DataScanner.UrlsGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
            try
            {
                Log.Information("UrlsGenerator service started");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    UrlsGeneratorBindingSettings bindingSettings = new UrlsGeneratorBindingSettings();
                    IConfigurationSection bindingConfig = hostContext.Configuration.GetSection("Binding");                   
                    bindingConfig.Bind(bindingSettings);

                    RmqBuilderConnSettings connSettings = new RmqBuilderConnSettings();
                    IConfigurationSection connConfig = hostContext.Configuration.GetSection("RmqConnectionSettings");                    
                    connConfig.Bind(connSettings);
                    
                    services.Configure<UrlsGeneratorAppSettings>(hostContext.Configuration.GetSection("Application"));
                    services.Configure<UrlsGeneratorBindingSettings>(bindingConfig);
                    services.AddSingleton(new RmqPublisherBuilder()                       
                        .UsingExchange(bindingSettings.SenderExchange)
                        .UsingConfigConnectionSettings(connSettings)
                        .Build());                   
                    services.AddHostedService<UrlsGeneratorService>();
                });
    }
}
