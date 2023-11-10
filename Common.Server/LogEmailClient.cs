using Microsoft.Extensions.Logging;

namespace Common.Server;

public class LogEmailClient : IEmailClient
{
    private readonly ILogger<LogEmailClient> _log;

    public LogEmailClient(ILogger<LogEmailClient> log)
    {
        _log = log;
    }

    public async Task SendEmailAsync(
        string subject,
        string bodyHtml,
        string bodyText,
        string senderAddress,
        List<string> toAddresses,
        List<string>? ccAddresses = null,
        List<string>? bccAddresses = null,
        CancellationToken ctkn = default
    )
    {
        _log.LogInformation(
            $"Sending Email:\nsubject: {subject}\nbodyHtml: {bodyHtml}\nbodyText: {bodyText}\nsenderAddress: {senderAddress}\ntoAddress: {string.Join(", ", toAddresses)}\nccAddress: {string.Join(", ", ccAddresses ?? new List<string>())}\nbccAddress: {string.Join(", ", bccAddresses ?? new List<string>())}"
        );
    }

    public void Dispose() { }
}
