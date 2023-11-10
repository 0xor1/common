namespace Common.Server;

public interface IEmailClient : IDisposable
{
    public Task SendEmailAsync(
        string subject,
        string bodyHtml,
        string bodyText,
        string senderAddress,
        List<string> toAddresses,
        List<string>? ccAddresses = null,
        List<string>? bccAddresses = null,
        CancellationToken ctkn = default
    );
}
