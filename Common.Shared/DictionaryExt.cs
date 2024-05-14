namespace Common.Shared;

public record MaybeGot<T>(T? Val, bool Got);

public static class DictionaryExt
{
    public static MaybeGot<TVal?> Try<TKey, TVal>(this IDictionary<TKey, TVal>? src, TKey key)
        where TVal : struct
    {
        if (src == null)
        {
            return new MaybeGot<TVal?>(null, false);
        }
        var got = src.TryGetValue(key, out TVal val);
        if (got)
        {
            return new MaybeGot<TVal?>(val, true);
        }
        return new MaybeGot<TVal?>(null, false);
    }

    public static MaybeGot<TVal?> TryRef<TKey, TVal>(this IDictionary<TKey, TVal>? src, TKey key)
        where TVal : class
    {
        if (src == null)
        {
            return new MaybeGot<TVal?>(null, false);
        }
        var got = src.TryGetValue(key, out TVal? val);
        if (got)
        {
            return new MaybeGot<TVal?>(val, true);
        }
        return new MaybeGot<TVal?>(null, false);
    }
}
