namespace Common.Server;

public class Pwd
{
    public int PwdVersion { get; set; }
    public byte[] PwdSalt { get; set; }
    public byte[] PwdHash { get; set; }
    public int PwdIters { get; set; }
}