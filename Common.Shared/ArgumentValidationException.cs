namespace Common.Shared;

public class ArgumentValidationException : Exception
{
    public ArgumentValidationException(string msg)
        : base(msg) { }
}
