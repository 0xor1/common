using System.Security.Cryptography;
using Common.Shared;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Common.Server;

public static class Crypto
{
    public static byte[] Bytes(int n)
    {
        var random = new byte[n];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(random);
        return random;
    }

    public static string String(int n)
    {
        return Bytes(n).ToB64();
    }

    public static Pwd HashPwd(string pwd)
    {
        return HashPwdV1(pwd, Bytes(16), 100000, 32);
    }

    public static bool PwdIsValid(string @try, Pwd pwd)
    {
        var attempt = HashPwdV1(@try, pwd.PwdSalt, pwd.PwdIters, pwd.PwdHash.Length);
        return pwd.PwdHash.SequenceEqual(attempt.PwdHash);
    }

    private static Pwd HashPwdV1(string pwd, byte[] salt, int iters, int hashLen)
    {
        return new Pwd
        {
            PwdVersion = 1,
            PwdSalt = salt,
            PwdHash = KeyDerivation.Pbkdf2(pwd, salt, KeyDerivationPrf.HMACSHA512, iters, hashLen),
            PwdIters = iters
        };
    }
}