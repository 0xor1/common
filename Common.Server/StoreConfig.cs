using Amazon;
using Newtonsoft.Json;

namespace Common.Server;

public record StoreConfig
{
    public string Host { get; init; }
    public string Region { get; init; }
    public string Key { get; init; }
    public string Secret { get; init; }

    [JsonIgnore]
    public RegionEndpoint RegionEndpoint => Region.GetRegionEndpoint();
}
