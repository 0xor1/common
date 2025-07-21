using Common.Server.Auth;
using Common.Shared;
using Common.Shared.Auth;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Message = FirebaseAdmin.Messaging.Message;
using S = Common.Shared.I18n.S;

namespace Common.Server;

public class FcmClient : IFcmClient
{
    private readonly FirebaseMessaging _client;

    // fcm multicast message endpoint has a max of 500 token limit
    private const int _batchSize = 500;

    public FcmClient(FirebaseMessaging client)
    {
        _client = client;
    }

    public async Task Send(Message msg, bool fnf = true, CancellationToken ctkn = default)
    {
        var t = _client.SendAsync(msg, ctkn);
        if (fnf)
        {
            t.FnF();
        }
        else
        {
            await t;
        }
    }

    public async Task SendTopic<TDbCtx>(
        IRpcCtx ctx,
        TDbCtx db,
        ISession ses,
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
        var tokens = await db
            .FcmRegs.Where(x => x.Topic == topicStr && x.FcmEnabled && x.Client != client)
            .Select(x => x.Token)
            .ToListAsync(ctx.Ctkn);
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
            { Fcm.ClientHeaderName, ctx.GetHeader(Fcm.ClientHeaderName) ?? "" },
        };
        if (data != null)
        {
            dic.Add(Fcm.Data, Json.From(data));
        }

        foreach (var t in tokens)
        {
            await Send(new() { Token = t, Data = dic }, fnf, ctx.Ctkn);
        }
    }
}
