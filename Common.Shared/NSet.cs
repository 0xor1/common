using MessagePack;

namespace Common.Shared;

// nullable setter / optional setter
// example:
// public record Update(string Id, NSet<string>? Name, NSet<int>? Age);
// this means you can tell if something is being set or not
// and whether it is being set to null or a value
[MessagePackObject]
public record NSet<T>
{
    [Key(0)]
    public T? V { get; set; }

    [SerializationConstructor]
    public NSet(T? v)
    {
        V = v;
    }
}
