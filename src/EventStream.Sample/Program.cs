using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitLink;
using RabbitLink.Logging;
using RabbitLink.Serialization.Json;

namespace EventStream.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }


        private class LinkLoggerFactory : ILinkLoggerFactory
        {
            private readonly ILoggerFactory _factory;

            public LinkLoggerFactory(ILoggerFactory factory)
            {
                _factory = factory;
            }

            public ILinkLogger CreateLogger(string name)
            {
                return new LinkLogger(_factory.CreateLogger($"RabbitLink.{name}"));
            }

            private class LinkLogger : ILinkLogger
            {
                private readonly ILogger _logger;

                public LinkLogger(ILogger logger)
                {
                    _logger = logger;
                }

                public void Dispose()
                {
                    
                }

                public void Write(LinkLoggerLevel level, string message)
                {
                    switch (level)
                    {
                        case LinkLoggerLevel.Error:
                            _logger.LogError(message);
                            break;
                        case LinkLoggerLevel.Warning:
                            _logger.LogWarning(message);
                            break;
                        case LinkLoggerLevel.Info:
                            _logger.LogInformation(message);
                            break;
                        case LinkLoggerLevel.Debug:
                            _logger.LogDebug(message);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(level), level, null);
                    }
                }
            }
        }

        private static ILink CreateLink(IServiceProvider services)
        {
            var configurationString = services.GetService<IConfiguration>().GetValue<string>("RabbitMq:ConnectionString");
            var loggerFactory = services.GetService<ILoggerFactory>();


            return LinkBuilder
                .Configure
                .AppId("EventStream.Sample")
                .Uri(configurationString)
                .LoggerFactory(new LinkLoggerFactory(loggerFactory))
                .Serializer(new LinkJsonSerializer())
                .Build();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(CreateLink);
                    services.AddSingleton<IMessageProducer, MessageProducer>();

                    services.AddHostedService<Worker>();
                    
                });
    }
}
