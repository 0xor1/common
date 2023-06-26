using Amazon.Runtime;
using Amazon.S3;
using Amazon.SimpleEmail;
using Common.Server.Auth;
using Common.Shared;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using MySqlConnector;

namespace Common.Server;

public static class ServiceExts
{
    private static SemaphoreSlim _ss = new(1, 1);
    private static FirebaseMessaging? fbm = null;

    public static void AddApiServices<TDbCtx>(
        this IServiceCollection services,
        IConfig config,
        S s,
        Action<IServiceCollection>? addServices = null,
        Func<IServiceProvider, Task>? initApp = null
    )
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
                            ServiceURL = $"http://{config.Store.Host}",
                            ForcePathStyle = true
                        }
                    )
            );
            services.AddScoped<IMinioClient>(
                sp =>
                    new MinioClient()
                        .WithEndpoint(config.Store.Host)
                        .WithCredentials(config.Store.Key, config.Store.Secret)
                        .Build()
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
            services.AddScoped<IMinioClient>(
                sp =>
                    new MinioClient()
                        .WithRegion(config.Store.Region)
                        .WithCredentials(config.Store.Key, config.Store.Secret)
                        .Build()
            );
        }
        services.AddScoped<IStoreClient, S3StoreClient>();

        if (
            !config.Fcm.ServiceAccountKeyFile.IsNullOrWhiteSpace()
            && File.Exists(config.Fcm.ServiceAccountKeyFile)
        )
        {
            // if an fcm service account key file is specified then use it
            InitFirebaseMessaging(config);
            services.AddSingleton(fbm.NotNull());
            services.AddSingleton<IFcmClient, FcmClient>();
        }
        else
        {
            // otherwise use nop
            services.AddSingleton<IFcmClient, FcmNopClient>();
        }

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

        addServices?.Invoke(services);

        if (initApp != null)
        {
            using var sp = services.BuildServiceProvider();
            var t = initApp(sp);
            t.Wait();
            if (t.Exception != null)
            {
                throw t.Exception;
            }
        }
    }

    private static void InitFirebaseMessaging(IConfig config)
    {
        if (fbm == null)
        {
            _ss.Wait();
            if (fbm == null)
            {
                fbm = FirebaseMessaging.GetMessaging(
                    FirebaseApp.Create(
                        new AppOptions()
                        {
                            Credential = GoogleCredential.FromFile(config.Fcm.ServiceAccountKeyFile)
                        }
                    )
                );
            }
            _ss.Release();
        }
    }
}
