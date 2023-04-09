namespace Common.Shared;

public static class Id
{
    public static string New()
    {
        return Ulid.NewUlid().ToByteArray().ToB64();
    }
}
