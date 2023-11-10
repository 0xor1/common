using Microsoft.EntityFrameworkCore;

namespace Common.Server.Auth;

[PrimaryKey(nameof(Topic), nameof(Token))]
public class FcmReg
{
    public string Topic { get; set; }
    public string Token { get; set; }
    public string User { get; set; }
    public string Client { get; set; }
    public DateTime CreatedOn { get; set; }

    public bool FcmEnabled { get; set; }
}
