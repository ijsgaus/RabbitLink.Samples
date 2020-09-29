using System.Threading;
using System.Threading.Tasks;

namespace EventStream.Sample
{
    public interface IMessageProducer
    {
        Task PublishAsync(SampleMessage message, CancellationToken token);
    }
}