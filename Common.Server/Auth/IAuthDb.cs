using Microsoft.EntityFrameworkCore;

namespace Common.Server.Auth;

public interface IAuthDb
{
    DbSet<Auth> Auths { get; }
    DbSet<FcmReg> FcmRegs { get; }
}
