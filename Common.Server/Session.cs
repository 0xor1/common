using System.Net;
using System.Security;
using System.Security.Cryptography;
using Common.Shared;
using MessagePack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Server;

[MessagePackObject]
public record Session
{
    [Key(0)]
    public string Id { get; init; }

    [Key(1)]
    public bool IsAuthed { get; init; }

    [Key(2)]
    public DateTime StartedOn { get; init; }

    [Key(3)]
    public bool RememberMe { get; init; }

    [Key(4)]
    public string Lang { get; init; }

    [Key(5)]
    public string DateFmt { get; init; }

    [Key(6)]
    public string TimeFmt { get; init; }

    [IgnoreMember]
    public bool IsAnon => !IsAuthed;

    public Shared.Auth.Session ToApiSession()
    {
        return new(Id, IsAuthed, StartedOn, RememberMe, Lang, DateFmt, TimeFmt);
    }
}

[MessagePackObject]
public record SignedSession
{
    [Key(0)]
    public byte[] Session { get; init; }

    [Key(1)]
    public byte[] Signature { get; init; }
}

public interface ISessionManager
{
    private static ISessionManager? _inst;
    internal Session Get(HttpContext ctx);

    internal Session Create(
        HttpContext ctx,
        string userId,
        bool isAuthed,
        bool rememberMe,
        string lang,
        string dateFmt,
        string timeFmt
    );

    internal Session Clear(HttpContext ctx);

    public static ISessionManager Init(IReadOnlyList<string> signatureKeys)
    {
        return _inst ??= new SessionManager(signatureKeys);
    }
}

internal record SessionManager : ISessionManager
{
    private const string SessionKey = "s";
    private readonly byte[][] SignatureKeys;

    internal SessionManager(IReadOnlyList<string> signatureKeys)
    {
        SignatureKeys = signatureKeys.Select(x => x.FromB64()).ToArray();
        if (SignatureKeys.Count(x => x.Length != 64) > 0)
            throw new InvalidDataException(
                "config: all session signature keys must be 64 bytes long"
            );

        if (SignatureKeys.Length == 0)
            throw new InvalidDataException(
                "config: there must be at least 1 session signature key"
            );
    }

    public Session Get(HttpContext ctx)
    {
        Session ses;
        if (!ctx.Items.ContainsKey(SessionKey))
        {
            ses = GetCookie(ctx);
            ctx.Items[SessionKey] = ses;
        }
        else
        {
            ses = (Session)ctx.Items[SessionKey].NotNull();
        }

        return ses;
    }

    public Session Create(
        HttpContext ctx,
        string userId,
        bool isAuthed,
        bool rememberMe,
        string lang,
        string dateFmt,
        string timeFmt
    )
    {
        var ses = new Session
        {
            Id = userId,
            StartedOn = DateTime.UtcNow,
            IsAuthed = isAuthed,
            RememberMe = rememberMe,
            Lang = lang,
            DateFmt = dateFmt,
            TimeFmt = timeFmt
        };
        ctx.Items[SessionKey] = ses;
        SetCookie(ctx, ses);
        return ses;
    }

    public Session Clear(HttpContext ctx)
    {
        var ses = _Clear(ctx);
        ctx.Items[SessionKey] = ses;
        return ses;
    }

    private Session _Clear(HttpContext ctx)
    {
        var s = ctx.Get<S>();
        return Create(
            ctx,
            Id.New(),
            false,
            false,
            s.BestLang(ctx.Request.Headers.AcceptLanguage.ToArray().FirstOrDefault() ?? ""),
            s.DefaultDateFmt,
            s.DefaultTimeFmt
        );
    }

    private Session GetCookie(HttpContext ctx)
    {
        var c = ctx.Request.Cookies[SessionKey];
        if (c.IsNull())
            // there is no session set so use sign out to create a
            // new anon session
            return _Clear(ctx);

        // there is a session so lets get it from the cookie

        var signedSes = MessagePackSerializer.Deserialize<SignedSession>(c.FromB64());
        var i = 0;
        foreach (var signatureKey in SignatureKeys)
            using (var hmac = new HMACSHA256(signatureKey))
            {
                var sesSig = hmac.ComputeHash(signedSes.Session);
                if (sesSig.SequenceEqual(signedSes.Signature))
                {
                    var ses = MessagePackSerializer.Deserialize<Session>(signedSes.Session);
                    if (i > 0)
                        // if it wasnt signed using the latest key, resign the cookie using the latest key
                        SetCookie(ctx, ses);
                    return ses;
                }

                i++;
            }

        throw new SecurityException("Session signature verification failed");
    }

    private void SetCookie(HttpContext ctx, Session ses)
    {
        // turn session into bytes
        var sesBytes = MessagePackSerializer.Serialize(ses);
        // sign the session
        byte[] sesSig;
        using (var hmac = new HMACSHA256(SignatureKeys.First()))
        {
            sesSig = hmac.ComputeHash(sesBytes);
        }

        // create the cookie value with the session and signature
        var signedSes = new SignedSession { Session = sesBytes, Signature = sesSig };
        // get final cookie bytes
        var cookieBytes = MessagePackSerializer.Serialize(signedSes);
        // create cookie
        ctx.Response.Cookies.Append(
            SessionKey,
            cookieBytes.ToB64(),
            new CookieOptions
            {
                Secure = true,
                HttpOnly = true,
                IsEssential = true,
                Expires = ses.RememberMe ? DateTime.UtcNow.AddDays(7) : null,
                SameSite = SameSiteMode.Strict
            }
        );
    }
}

public static class HttpContextExts
{
    // these require that ISessionManager was added to service container
    public static T Get<T>(this HttpContext ctx)
        where T : notnull
    {
        return ctx.RequestServices.GetRequiredService<T>();
    }

    private static ISessionManager GetSessionManager(this HttpContext ctx)
    {
        return ctx.Get<ISessionManager>();
    }

    public static Session GetSession(this HttpContext ctx)
    {
        return ctx.GetSessionManager().Get(ctx);
    }

    public static Session CreateSession(
        this HttpContext ctx,
        string userId,
        bool isAuthed,
        bool rememberMe,
        string lang,
        string dateFmt,
        string timeFmt
    )
    {
        return ctx.GetSessionManager()
            .Create(ctx, userId, isAuthed, rememberMe, lang, dateFmt, timeFmt);
    }

    public static Session ClearSession(this HttpContext ctx)
    {
        return ctx.GetSessionManager().Clear(ctx);
    }

    public static void ErrorIf(
        this HttpContext ctx,
        bool condition,
        string key,
        object? model = null,
        HttpStatusCode code = HttpStatusCode.InternalServerError
    )
    {
        Throw.If(condition, () => new RpcException(ctx.String(key, model), code));
    }

    public static void ErrorFromValidationResult(
        this HttpContext ctx,
        ValidationResult res,
        HttpStatusCode code = HttpStatusCode.InternalServerError
    )
    {
        Throw.If(
            !res.Valid,
            () =>
                new RpcException(
                    $"{ctx.String(res.Message.Key, res.Message.Model)}{(res.SubMessages.Any() ? $":\n{string.Join("\n", res.SubMessages.Select(x => ctx.String(x.Key, x.Model)))}" : "")}",
                    code
                )
        );
    }

    public static string String(this HttpContext ctx, string key, object? model = null)
    {
        return ctx.Get<S>().GetOrAddress(ctx.GetSession().Lang, key, model);
    }
}
