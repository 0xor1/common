using Microsoft.AspNetCore.Http.Features;

namespace Common.Server.Test;

public class TestHttpMaxRequestBodySizeFeature : IHttpMaxRequestBodySizeFeature
{
    public bool IsReadOnly => false;
    public long? MaxRequestBodySize { get; set; }
}
