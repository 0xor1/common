namespace Common.Shared;

public class InvalidSetupException : Exception
{
    public InvalidSetupException(string msg)
        : base(msg) { }
}
