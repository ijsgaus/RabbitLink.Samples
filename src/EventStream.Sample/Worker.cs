using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitLink;
using RabbitLink.Consumer;
using RabbitLink.Messaging;

namespace EventStream.Sample
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMessageProducer _producer;
        private readonly ILink _link;

        public Worker(ILogger<Worker> logger, IMessageProducer producer, ILink link)
        {
            _logger = logger;
            _producer = producer;
            _link = link;
        }

        private async Task<LinkConsumerAckStrategy> OnMessage(ILinkConsumedMessage<SampleMessage> msg)
        {
            var payload = msg.Body;
            // any processing
            await _producer.PublishAsync(msg.Body, msg.Cancellation);
            return LinkConsumerAckStrategy.Ack;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = _link.Consumer
                .Queue(async cfg =>
                {
                    var exch = await cfg.ExchangeDeclarePassive("sample1");
                    var queue = await cfg.QueueDeclare("test.queue");
                    await cfg.Bind(queue, exch);
                    return queue;
                })
                .Handler<SampleMessage>(OnMessage)
                .PrefetchCount(10) // в 10 потоков
                .Build();
            stoppingToken.Register(() => consumer.Dispose());
        }
    }
}
