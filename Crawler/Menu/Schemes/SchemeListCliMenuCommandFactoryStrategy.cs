using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using Crawler.Cruders;
using LibCrawlerRepositories;

namespace Crawler.Menu.Schemes;

// ReSharper disable once ClassNeverInstantiated.Global
public class SchemeListCliMenuCommandFactoryStrategy : IMenuCommandFactoryStrategy
{
    private readonly ICrawlerRepository _crawlerRepository;

    public SchemeListCliMenuCommandFactoryStrategy(ICrawlerRepository crawlerRepository)
    {
        _crawlerRepository = crawlerRepository;
    }

    public CliMenuCommand CreateMenuCommand()
    {
        var schemeCruder = new SchemeCruder(_crawlerRepository);
        return new CruderListCliMenuCommand(schemeCruder);
    }
}
