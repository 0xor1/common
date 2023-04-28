using Newtonsoft.Json;

namespace Common.Shared;

public class ArgumentValidationException : Exception
{
    public ArgumentValidationException(string msg)
        : base(msg) { }
}

public class MinMaxBaseException : ArgumentValidationException
{
    public MinMaxBaseException(string msg)
        : base(msg) { }
}

public class NullMinMaxValuesException : MinMaxBaseException
{
    public NullMinMaxValuesException()
        : base("invalid min max args, both are null") { }
}

public class ReversedMinMaxValuesException : MinMaxBaseException
{
    public string Min { get; }
    public string Max { get; }

    public ReversedMinMaxValuesException(string min, string max)
        : base($"reversed min max args, min: {min} must not be larger than max: {max}")
    {
        Min = min;
        Max = max;
    }
}

public record MinMax<T>
    where T : IComparable<T>
{
    public T? Min { get; }
    public T? Max { get; }

    [JsonConstructor]
    public MinMax(T? min, T? max)
    {
        if (min == null && max == null)
        {
            throw new NullMinMaxValuesException();
        }
        if (min != null && max != null && min.CompareTo(max) > 0)
        {
            throw new ReversedMinMaxValuesException(
                min.ToString().NotNull(),
                max.ToString().NotNull()
            );
        }

        Min = min;
        Max = max;
    }
}
