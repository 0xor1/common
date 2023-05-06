﻿using Amazon.Runtime;
using Amazon.S3;
using Amazon.SimpleEmail;
using Common.Server.Auth;
using Common.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

namespace Common.Server;

public static class ServiceExts
{
    public static void AddApiServices<TDbCtx>(this IServiceCollection services, IConfig config, S s)
        where TDbCtx : DbContext, IAuthDb
    {
        services.AddLogging();
        services.AddSingleton(config);
        services.AddSingleton(s);
        services.AddSingleton<IRpcHttpSessionManager>(
            new RpcHttpSessionManager(config.Session.SignatureKeys, s)
        );
        if (config.Env == Env.Lcl)
        {
            services.AddScoped<IEmailClient, LogEmailClient>();
            services.AddScoped<AmazonS3Client>(
                sp =>
                    new AmazonS3Client(
                        new BasicAWSCredentials(config.Store.Key, config.Store.Secret),
                        // this is needed to work with minio locally
                        new AmazonS3Config()
                        {
                            ServiceURL = config.Store.Region,
                            ForcePathStyle = true
                        }
                    )
            );
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
            services.AddScoped<AmazonS3Client>(
                sp =>
                    // this is for running in prod against actual aws s3
                    new AmazonS3Client(
                        new BasicAWSCredentials(config.Store.Key, config.Store.Secret),
                        config.Store.RegionEndpoint
                    )
            );
        }
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
