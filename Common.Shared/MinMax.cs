using MessagePack;

namespace Common.Shared;

[MessagePackObject]
public record MinMax<T>
    where T : struct, IComparable<T>
{
    private T? _min;
    private T? _max;

    [Key(0)]
    public T? Min
    {
        get => _min;
        set
        {
            if (value.HasValue && Max.HasValue && value.Value.CompareTo(Max.Value) > 0)
            {
                throw new ReversedMinMaxValuesException(
                    value.ToString().NotNull(),
                    Max.ToString().NotNull()
                );
            }
            _min = value;
        }
    }

    [Key(1)]
    public T? Max
    {
        get => _max;
        set
        {
            if (value.HasValue && Min.HasValue && Min.Value.CompareTo(value.Value) > 0)
            {
                throw new ReversedMinMaxValuesException(
                    Min.ToString().NotNull(),
                    value.ToString().NotNull()
                );
            }
            _max = value;
        }
    }

    [SerializationConstructor]
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
