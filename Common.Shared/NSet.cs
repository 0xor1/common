namespace Common.Shared;

// nullable setter / optional setter
// example:
// public record Update(string Id, NSet<string>? Name, NSet<int>? Age);
// this means you can tell if something is being set or not
// and whether it is being set to null or a value
public record NSet<T>(T? V);
