using MessagePack;

namespace Common.Shared;

[MessagePackObject]
public record Maybe<T>
{
    [SerializationConstructor]
    public Maybe(T? item)
    {
        Item = item;
    }

    [Key(0)]
    public T? Item { get; set; }
}
