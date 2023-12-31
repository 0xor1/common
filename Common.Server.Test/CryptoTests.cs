using System.Text;
using Amazon.S3;
using Common.Server.Auth;
using Common.Shared;
using Common.Shared.Auth;
using Xunit.Abstractions;

namespace Common.Server.Test;

public class CryptoTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CryptoTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task String_Success()
    {
        // used for generating cookie session signature keys
        var s = Crypto.String(64);
        _testOutputHelper.WriteLine(s);
    }
}
