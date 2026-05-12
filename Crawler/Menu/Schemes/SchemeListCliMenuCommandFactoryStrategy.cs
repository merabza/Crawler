using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using Crawler.Cruders;
using LibCrawlerRepositories;

namespace Crawler.Menu.Schemes;

// ReSharper disable once ClassNeverInstantiated.Global
public class SchemeListCliMenuCommandFactoryStrategy(ICrawlerRepository crawlerRepository) : IMenuCommandFactoryStrategy
{
    public CliMenuCommand CreateMenuCommand()
    {
        var schemeCruder = new SchemeCruder(crawlerRepository);
        return new CruderListCliMenuCommand(schemeCruder);
    }
}
