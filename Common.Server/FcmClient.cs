using FirebaseAdmin.Messaging;

namespace Common.Server;

public interface IFcmClient
{
    Task<IBatchResponse> Send(MulticastMessage msg);
}

public class FcmClient : IFcmClient
{
    private readonly FirebaseMessaging _client;

    // fcm multicast message endpoint has a max of 500 token limit
    private const int _batchSize = 500;

    public FcmClient(FirebaseMessaging client)
    {
        _client = client;
    }

    public async Task<IBatchResponse> Send(MulticastMessage msg)
    {
        if (!msg.Tokens.Any())
        {
            return new NopBatchResponse(new List<SendResponse>(), 0);
        }

        var allTokens = msg.Tokens.ToList();
        var i = 0;
        var successCount = 0;
        var responses = new List<SendResponse>();
        while (i < allTokens.Count)
        {
            var batch = allTokens.GetRange(i, Math.Min(i + _batchSize, allTokens.Count));
            msg.Tokens = batch;
            var res = await _client.SendMulticastAsync(msg);
            responses.AddRange(res.Responses);
            successCount += res.SuccessCount;
            i += _batchSize;
        }

        return new BatchResponseWrapper(responses, successCount);
    }
}

public class FcmNopClient : IFcmClient
{
    public FcmNopClient() { }

    public async Task<IBatchResponse> Send(MulticastMessage msg)
    {
        await Task.CompletedTask;
        return new NopBatchResponse(new List<SendResponse>(msg.Tokens.Count), msg.Tokens.Count);
    }
}

public interface IBatchResponse
{
    public IReadOnlyList<SendResponse> Responses { get; }
    public int SuccessCount { get; }
    public int FailureCount => Responses.Count - SuccessCount;
}

public class BatchResponseWrapper : IBatchResponse
{
    private readonly List<SendResponse> _responses;
    private readonly int _successCount;

    internal BatchResponseWrapper(List<SendResponse> responses, int successCount)
    {
        _responses = responses;
        _successCount = successCount;
    }

    public IReadOnlyList<SendResponse> Responses => _responses;
    public int SuccessCount => _successCount;
}

public class NopBatchResponse : IBatchResponse
{
    public NopBatchResponse(IReadOnlyList<SendResponse> responses, int successCount)
    {
        Responses = responses;
        SuccessCount = successCount;
    }

    public IReadOnlyList<SendResponse> Responses { get; }
    public int SuccessCount { get; }
}
