using Amazon;
using Common.Shared;

namespace Common.Server;

public static class AwsStringExts
{
    public static RegionEndpoint GetRegionEndpoint(this string region)
    {
        var re = RegionEndpoint.GetBySystemName(region);
        if (re == null)
            throw new InvalidSetupException(
                $"couldn't find aws region endpoint with system name: {region}"
            );
        return re;
    }
}
