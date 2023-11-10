using Newtonsoft.Json;

namespace Common.Shared;

public record MinMax<T>
    where T : struct, IComparable<T>
{
    public T? Min { get; }
    public T? Max { get; }

    [JsonConstructor]
    public MinMax(T? min, T? max)
    {
        if (!min.HasValue && !max.HasValue)
        {
            throw new NullMinMaxValuesException();
        }
        if (min.HasValue && max.HasValue && min.Value.CompareTo(max.Value) > 0)
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
