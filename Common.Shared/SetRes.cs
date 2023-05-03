namespace Common.Shared;

// used for returning a paginated set of results with the more flag
// to indicate if there are any more items in the set beyond what
// was returned 
public record SetRes<T>(IReadOnlyList<T> Set, bool More);
