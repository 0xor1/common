using MessagePack;

namespace Common.Shared;

// used for returning a paginated set of results with the more flag
// to indicate if there are any more items in the set beyond what
// was returned

[MessagePackObject]
public record SetRes<T>
{
    [SerializationConstructor]
    public SetRes(List<T> set, bool more)
    {
        Set = set;
        More = more;
    }

    [Key(0)]
    public List<T> Set { get; set; }

    [Key(1)]
    public bool More { get; set; }

    // qryLimit should always be the desired limit plus 1 to
    // work out if there are any more pages of items to look up.
    public static SetRes<T> FromLimit(List<T> set, int qryLimit)
    {
        if (set.Count == qryLimit)
        {
            set.RemoveAt(qryLimit - 1);
            return new SetRes<T>(set, true);
        }
        return new SetRes<T>(set, false);
    }
}
