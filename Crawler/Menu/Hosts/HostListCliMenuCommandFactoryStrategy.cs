using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using Crawler.Cruders;
using LibCrawlerRepositories;

namespace Crawler.Menu.Hosts;

// ReSharper disable once ClassNeverInstantiated.Global
public class HostListCliMenuCommandFactoryStrategy : IMenuCommandFactoryStrategy
{
    private readonly ICrawlerRepository _crawlerRepository;

    public HostListCliMenuCommandFactoryStrategy(ICrawlerRepository crawlerRepository)
    {
        _crawlerRepository = crawlerRepository;
    }

    public CliMenuCommand CreateMenuCommand()
    {
        //ჰოსტების რედაქტორი
        var hostCruder = new HostCruder(_crawlerRepository);
        //"Hosts"
        return new CruderListCliMenuCommand(hostCruder);
    }
}
