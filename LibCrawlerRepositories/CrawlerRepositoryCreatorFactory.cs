using System;
using Microsoft.Extensions.DependencyInjection;

namespace LibCrawlerRepositories;

public sealed class CrawlerRepositoryCreatorFactory : ICrawlerRepositoryCreatorFactory
{
    private readonly IServiceProvider _services;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CrawlerRepositoryCreatorFactory(IServiceProvider services)
    {
        _services = services;
    }

    public ICrawlerRepository GetCrawlerRepository()
    {
        // ReSharper disable once using
        var scope = _services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ICrawlerRepository>();
    }
}