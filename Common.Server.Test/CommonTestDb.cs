using Common.Server.Auth;
using Microsoft.EntityFrameworkCore;

namespace Common.Server.Test;

public class CommonTestDb : DbContext, IAuthDb
{
    public CommonTestDb(DbContextOptions<CommonTestDb> opts)
        : base(opts) { }

    public DbSet<Auth.Auth> Auths { get; set; } = null!;
    public DbSet<FcmReg> FcmRegs { get; set; } = null!;
}
