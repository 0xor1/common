namespace Common.Shared;

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
