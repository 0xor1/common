using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Logging;

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
        List<string>? bccAddresses = null
    );
}

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
        List<string>? bccAddresses = null
    )
    {
        _log.LogInformation(
            $"Sending Email:\nsubject: {subject}\nbodyHtml: {bodyHtml}\nbodyText: {bodyText}\nsenderAddress: {senderAddress}\ntoAddress: {string.Join(", ", toAddresses)}\nccAddress: {string.Join(", ", ccAddresses ?? new List<string>())}\nbccAddress: {string.Join(", ", bccAddresses ?? new List<string>())}"
        );
    }

    public void Dispose() { }
}

public class SesEmailClient : IEmailClient
{
    private readonly AmazonSimpleEmailServiceClient _awsSes;

    public SesEmailClient(AmazonSimpleEmailServiceClient awsSes)
    {
        _awsSes = awsSes;
    }

    public async Task SendEmailAsync(
        string subject,
        string bodyHtml,
        string bodyText,
        string senderAddress,
        List<string> toAddresses,
        List<string>? ccAddresses = null,
        List<string>? bccAddresses = null
    )
    {
        var response = await _awsSes.SendEmailAsync(
            new SendEmailRequest
            {
                Destination = new Destination
                {
                    BccAddresses = bccAddresses,
                    CcAddresses = ccAddresses,
                    ToAddresses = toAddresses
                },
                Message = new Message
                {
                    Body = new Body
                    {
                        Html = new Content { Charset = "UTF-8", Data = bodyHtml },
                        Text = new Content { Charset = "UTF-8", Data = bodyText }
                    },
                    Subject = new Content { Charset = "UTF-8", Data = subject }
                },
                Source = senderAddress
            }
        );
    }

    public void Dispose()
    {
        _awsSes.Dispose();
    }
}
