using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SimpleEmail;
using Common.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

namespace Common.Server;

public static class ServiceExts
{
    public static void AddApiServices<TDbCtx>(this IServiceCollection services, Config config, S s)
        where TDbCtx : DbContext
    {
        services.AddSingleton(s);
        services.AddSingleton(ISessionManager.Init(config.Session.SignatureKeys));
        if (config.Env == Env.LCL)
        {
            services.AddScoped<IEmailClient, LogEmailClient>();
        }
        else
        {
            services.AddScoped<AmazonSimpleEmailServiceClient>(
                sp =>
                    new AmazonSimpleEmailServiceClient(
                        new BasicAWSCredentials(config.Email.Key, config.Email.Secret),
                        config.Email.RegionEndpoint
                    )
            );
            services.AddScoped<IEmailClient, SesEmailClient>();
        }
        services.AddScoped<AmazonS3Client>(
            sp =>
                new AmazonS3Client(
                    new BasicAWSCredentials(config.Store.Key, config.Store.Secret),
                    config.Store.RegionEndpoint
                )
        );
        services.AddScoped<IStoreClient, S3StoreClient>();
        services.AddDbContext<TDbCtx>(dbContextOptions =>
        {
            var cnnStrBldr = new MySqlConnectionStringBuilder(config.Db.Connection);
            cnnStrBldr.Pooling = true;
            cnnStrBldr.MaximumPoolSize = 100;
            cnnStrBldr.MinimumPoolSize = 1;
            var version = new MariaDbServerVersion(new Version(10, 8));
            dbContextOptions.UseMySql(
                cnnStrBldr.ToString(),
                version,
                opts =>
                {
                    opts.CommandTimeout(1);
                }
            );
        });
    }
}
