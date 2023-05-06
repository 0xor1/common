using FirebaseAdmin.Messaging;

namespace Common.Server;

public interface IFcmClient
{
    Task<IBatchResponse> Send(MulticastMessage msg);
}

public class FcmClient : IFcmClient
{
    private readonly FirebaseMessaging _client;
    public FcmClient(FirebaseMessaging client)
    {
        _client = client;
    }

    public async Task<IBatchResponse> Send(MulticastMessage msg)
        => new BatchResponseWrapper(await _client.SendMulticastAsync(msg));
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
    private readonly BatchResponse _br;

    internal BatchResponseWrapper(BatchResponse br)
    {
        _br = br;
    }
    public IReadOnlyList<SendResponse> Responses => _br.Responses;
    public int SuccessCount => _br.SuccessCount;
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