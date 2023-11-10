namespace Common.Shared;

public record Lang(string Code, string NativeName)
{
    public override string ToString()
    {
        return NativeName;
    }
}
