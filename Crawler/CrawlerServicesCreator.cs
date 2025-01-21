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

        var (devDataProvider, devConnectionString) =
            DbConnectionFabric.GetDataProviderAndConnectionString(_par.DatabaseParameters,
                databaseServerConnections);





        if (!string.IsNullOrEmpty(_par.ConnectionString))
            services.AddDbContext<CrawlerDbContext>(options => options.UseSqlServer(_par.ConnectionString));

        services.AddScoped<ICrawlerRepository, CrawlerRepository>();
        services.AddSingleton<ICrawlerRepositoryCreatorFabric, CrawlerRepositoryCreatorFabric>();
        services.AddHttpClient();
    }
}