using System;
using CliParametersDataEdit;
using CrawlerDb;
using DoCrawler.Models;
using LibCrawlerRepositories;
using LibDatabaseParameters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SystemToolsShared;

namespace Crawler;

public sealed class CrawlerServicesCreator : ServicesCreator
{
    private readonly CrawlerParameters _par;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CrawlerServicesCreator(CrawlerParameters par) : base(par.LogFolder, null, "Crawler")
    {
        _par = par;
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        DatabaseServerConnections databaseServerConnections = new(_par.DatabaseServerConnections);

        var (dataProvider, connectionString) =
            DbConnectionFactory.GetDataProviderAndConnectionString(_par.DatabaseParameters, databaseServerConnections);

        if (!string.IsNullOrEmpty(connectionString))
            switch (dataProvider)
            {
                case EDatabaseProvider.SqlServer:
                    services.AddDbContext<CrawlerDbContext>(options => options.UseSqlServer(connectionString));
                    break;
                case EDatabaseProvider.None:
                case EDatabaseProvider.SqLite:
                case EDatabaseProvider.OleDb:
                case EDatabaseProvider.WebAgent:
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataProvider));
            }

        services.AddScoped<ICrawlerRepository, CrawlerRepository>();
        services.AddSingleton<ICrawlerRepositoryCreatorFactory, CrawlerRepositoryCreatorFactory>();
        services.AddHttpClient();
    }
}