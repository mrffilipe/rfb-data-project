using System.Threading.Channels;
using RFBDataProject.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace RFBDataProject.Infrastructure.Ingestion.Pipeline;

public sealed class PipelineChannelFactory
{
    private readonly RfbOptions _options;

    public PipelineChannelFactory(IOptions<RfbOptions> options) => _options = options.Value;

    public Channel<T> Create<T>()
    {
        return Channel.CreateBounded<T>(new BoundedChannelOptions(_options.ChannelCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        });
    }
}
