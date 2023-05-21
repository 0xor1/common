using Common.Server.Auth;
using Common.Shared;
using Common.Shared.Auth;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Common.Server;

public interface IFcmClient
{
    Task<IBatchResponse> Send(MulticastMessage msg);
    Task SendTopic<TDbCtx>(
        IRpcCtx ctx,
        TDbCtx db,
        Session ses,
        IReadOnlyList<string> topic,
        object? data,
        bool fnf = true
    )
        where TDbCtx : IAuthDb;
    Task SendRaw(
        IRpcCtx ctx,
        FcmType type,
        IReadOnlyList<string> tokens,
        string topic,
        object? data,
        bool fnf = true
    );
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
        if (!(msg.Tokens?.Any() ?? false))
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

    public async Task SendTopic<TDbCtx>(
        IRpcCtx ctx,
        TDbCtx db,
        Session ses,
        IReadOnlyList<string> topic,
        object? data,
        bool fnf = true
    )
        where TDbCtx : IAuthDb
    {
        ctx.BadRequestIf(
            topic.Count < 1 || topic.Count > 5,
            S.AuthFcmTopicInvalid,
            new { Min = 1, Max = 5 }
        );
        var client = ctx.GetHeader(Fcm.ClientHeaderName) ?? "";
        var topicStr = Fcm.TopicString(topic);
        var tokens = await db.FcmRegs
            .Where(x => x.Topic == topicStr && x.FcmEnabled && x.Client != client)
            .Select(x => x.Token)
            .ToListAsync();
        await SendRaw(ctx, FcmType.Data, tokens, topicStr, data, fnf);
    }

    public async Task SendRaw(
        IRpcCtx ctx,
        FcmType type,
        IReadOnlyList<string> tokens,
        string topic,
        object? data,
        bool fnf = true
    )
    {
        if (!(tokens?.Any() ?? false))
        {
            return;
        }
        var dic = new Dictionary<string, string>()
        {
            { Fcm.TypeName, type.ToString() },
            { Fcm.Topic, topic },
            { Fcm.ClientHeaderName, ctx.GetHeader(Fcm.ClientHeaderName) ?? "" }
        };
        if (data != null)
        {
            dic.Add(Fcm.Data, Json.From(data));
        }
        var t = Send(new() { Tokens = tokens, Data = dic });
        if (fnf)
        {
            t.FnF();
        }
        else
        {
            await t;
        }
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

    public async Task SendTopic<TDbCtx>(
        IRpcCtx ctx,
        TDbCtx db,
        Session ses,
        IReadOnlyList<string> topic,
        object? data,
        bool fnf = true
    )
        where TDbCtx : IAuthDb
    {
        await Task.CompletedTask;
    }

    public async Task SendRaw(
        IRpcCtx ctx,
        FcmType type,
        IReadOnlyList<string> tokens,
        string topic,
        object? data,
        bool fnf = true
    )
    {
        await Task.CompletedTask;
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
