using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace Common.Server;

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
        List<string>? bccAddresses = null,
        CancellationToken ctkn = default
    )
    {
        var response = await _awsSes.SendEmailAsync(
            new SendEmailRequest
            {
                Destination = new Destination
                {
                    BccAddresses = bccAddresses,
                    CcAddresses = ccAddresses,
                    ToAddresses = toAddresses,
                },
                Message = new Message
                {
                    Body = new Body
                    {
                        Html = new Content { Charset = "UTF-8", Data = bodyHtml },
                        Text = new Content { Charset = "UTF-8", Data = bodyText },
                    },
                    Subject = new Content { Charset = "UTF-8", Data = subject },
                },
                Source = senderAddress,
            },
            ctkn
        );
    }

    public void Dispose()
    {
        _awsSes.Dispose();
    }
}
