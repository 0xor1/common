using Amazon;
using Newtonsoft.Json;

namespace Common.Server;

public record EmailConfig
{
    public EmailType Type { get; init; }
    public string Region { get; init; }
    public string Key { get; init; }
    public string Secret { get; init; }
    public string NoReplyAddress { get; init; }

    [JsonIgnore]
    public RegionEndpoint RegionEndpoint => Region.GetRegionEndpoint();
}

public enum EmailType
{
    Log,
    Ses
}
