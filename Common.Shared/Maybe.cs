using MessagePack;

namespace Common.Shared;

public record Maybe<T>
{
    [SerializationConstructor]
    public Maybe(T? item)
    {
        Item = Item;
    }

    [Key(0)]
    public T? Item { get; set; }
}
