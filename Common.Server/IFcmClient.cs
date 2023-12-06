using Common.Server.Auth;
using Common.Shared.Auth;
using FirebaseAdmin.Messaging;

namespace Common.Server;

public interface IFcmClient
{
    Task Send(Message msg, bool fnf = true, CancellationToken ctkn = default);
    Task SendTopic<TDbCtx>(
        IRpcCtx ctx,
        TDbCtx db,
        ISession ses,
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
