namespace Common.Shared;

public class NullMinMaxValuesException : MinMaxBaseException
{
    public NullMinMaxValuesException()
        : base("invalid min max args, both are null") { }
}
