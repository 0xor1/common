namespace Common.Shared;

public record Nothing
{
    public static readonly Type Type = typeof(Nothing);
    public static readonly Nothing Inst = new();

    private Nothing() { }
}
