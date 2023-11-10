using Common.Server.Auth;
using Common.Shared.Auth;
using FirebaseAdmin.Messaging;

namespace Common.Server;

public class FcmNopClient : IFcmClient
{
    public FcmNopClient() { }

    public async Task Send(Message msg, bool fnf = true, CancellationToken ctkn = default)
    {
        await Task.CompletedTask;
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
