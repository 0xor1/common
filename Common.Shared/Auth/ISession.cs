namespace Common.Shared.Auth;

public interface ISession
{
    string Id { get; }
    bool IsAuthed { get; }
    bool IsAnon => !IsAuthed;
    bool RememberMe { get; }
    string DateFmtStr => DateFmtExt.ToString(DateFmt, DateSeparator);
    string Lang { get; }
    DateFmt DateFmt { get; }
    string TimeFmt { get; }
    string DateSeparator { get; }
    string ThousandsSeparator { get; }
    string DecimalSeparator { get; }
    bool FcmEnabled { get; }

    public Session ToApi();
}
