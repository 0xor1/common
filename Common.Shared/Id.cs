
namespace Common.Shared;

public static class Id
{
    public static string New()
    {
         return Base64.UrlEncode(Ulid.NewUlid().ToByteArray());
    }
}