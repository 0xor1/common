namespace Common.Shared;

public record DateTimeFmt(string Value)
{
    private static readonly DateTime dt = new(DateTime.UtcNow.Year, 1, 21, 16, 1, 1);

    public override string ToString()
    {
        return dt.ToString(Value);
    }
}
