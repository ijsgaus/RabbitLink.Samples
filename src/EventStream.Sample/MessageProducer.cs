using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitLink;
using RabbitLink.Messaging;
using RabbitLink.Producer;
using RabbitLink.Topology;

namespace EventStream.Sample
{
    public class MessageProducer : IMessageProducer, IDisposable
    {
        private const string ExchangeName = "sample.exchange";

        private readonly ILinkProducer _producer;

        public MessageProducer(ILink link)
        {
            _producer = link.Producer
                .Exchange(cfg => cfg.ExchangeDeclare(ExchangeName, LinkExchangeType.Fanout))
                .ConfirmsMode(true)
                .Build();
        }

        public Task PublishAsync(SampleMessage message, CancellationToken token)
            => _producer.PublishAsync(new LinkPublishMessage<SampleMessage>(
                message,
                new LinkMessageProperties
                {
                    ContentType = "application/json", // это рюшечка
                    ContentEncoding = "utf8", // это рюшечка
                    DeliveryMode = LinkDeliveryMode.Persistent,
                    MessageId = Guid.NewGuid().ToString() // не обязательно, он по умолчанию так делает
                },
                new LinkPublishProperties
                {
                    Mandatory = true, // должна быть хотя бы одна очередь
                }
            ));

        public void Dispose()
        {
            _producer.Dispose();
        }
    }
}