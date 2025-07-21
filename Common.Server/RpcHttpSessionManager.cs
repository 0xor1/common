using System.Security.Cryptography;
using Common.Shared;
using MessagePack;
using Microsoft.AspNetCore.Http;
using ISession = Common.Shared.Auth.ISession;

namespace Common.Server;

public record RpcHttpSessionManager : IRpcHttpSessionManager
{
    private const string SessionKey = "s";
    private readonly byte[][] SignatureKeys;
    private readonly S _s;

    public RpcHttpSessionManager(IReadOnlyList<string> signatureKeys, S s)
    {
        _s = s;
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

    public ISession Get(HttpContext ctx)
    {
        ISession ses;
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

    public ISession Create(
        HttpContext ctx,
        string userId,
        bool isAuthed,
        bool rememberMe,
        string lang,
        DateFmt dateFmt,
        string timeFmt,
        string dateSeparator,
        string thousandsSeparator,
        string decimalSeparator,
        bool fcmEnabled
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
            TimeFmt = timeFmt,
            DateSeparator = dateSeparator,
            ThousandsSeparator = thousandsSeparator,
            DecimalSeparator = decimalSeparator,
            FcmEnabled = fcmEnabled,
        };
        ctx.Items[SessionKey] = ses;
        SetCookie(ctx, ses);
        return ses;
    }

    public ISession Clear(HttpContext ctx)
    {
        var ses = _Clear(ctx);
        ctx.Items[SessionKey] = ses;
        return ses;
    }

    private Session _Clear(HttpContext ctx)
    {
        return (Session)Create(
            ctx,
            Id.New(),
            false,
            false,
            _s.BestLang(ctx.Request.Headers.AcceptLanguage.ToArray().FirstOrDefault() ?? ""),
            _s.DefaultDateFmt,
            _s.DefaultTimeFmt,
            _s.DefaultDateSeparator,
            _s.DefaultThousandsSeparator,
            _s.DefaultDecimalSeparator,
            false
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

        try
        {
            var signedSes = MsgPck.To<SignedSession>(c.FromB64());
            var i = 0;
            foreach (var signatureKey in SignatureKeys)
                using (var hmac = new HMACSHA256(signatureKey))
                {
                    var sesSig = hmac.ComputeHash(signedSes.Session);
                    if (sesSig.SequenceEqual(signedSes.Signature))
                    {
                        var ses = MsgPck.To<Session>(signedSes.Session);
                        if (i > 0)
                            // if it wasnt signed using the latest key, resign the cookie using the latest key
                            SetCookie(ctx, ses);
                        return ses;
                    }

                    i++;
                }
        }
        catch (MessagePackSerializationException ex)
        {
            // session failed to deserialize so clear the session
            // so the user doesnt become blocked
            return _Clear(ctx);
        }

        // if we failed to validate the signature, just wipe to a new anon session
        return _Clear(ctx);
    }

    private void SetCookie(HttpContext ctx, Session ses)
    {
        // turn session into bytes
        var sesBytes = MsgPck.From(ses);
        // sign the session
        byte[] sesSig;
        using (var hmac = new HMACSHA256(SignatureKeys.First()))
        {
            sesSig = hmac.ComputeHash(sesBytes);
        }

        // create the cookie value with the session and signature
        var signedSes = new SignedSession { Session = sesBytes, Signature = sesSig };
        // get final cookie bytes
        var cookieBytes = MsgPck.From(signedSes);
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
                SameSite = SameSiteMode.Strict,
            }
        );
    }
}
