using System;
using Microsoft.Extensions.DependencyInjection;

namespace LibCrawlerRepositories;

public sealed class CrawlerRepositoryCreatorFabric : ICrawlerRepositoryCreatorFabric
{
    private readonly IServiceProvider _services;

    public CrawlerRepositoryCreatorFabric(IServiceProvider services)
    {
        _services = services;
    }

    public ICrawlerRepository GetCrawlerRepository()
    {
        var scope = _services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ICrawlerRepository>();
    }
}