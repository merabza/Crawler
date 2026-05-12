using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using Crawler.Cruders;
using LibCrawlerRepositories;

namespace Crawler.Menu.Hosts;

// ReSharper disable once ClassNeverInstantiated.Global
public class HostListCliMenuCommandFactoryStrategy(ICrawlerRepository crawlerRepository) : IMenuCommandFactoryStrategy
{
    public CliMenuCommand CreateMenuCommand()
    {
        //ჰოსტების რედაქტორი
        var hostCruder = new HostCruder(crawlerRepository);
        //"Hosts"
        return new CruderListCliMenuCommand(hostCruder);
    }
}
