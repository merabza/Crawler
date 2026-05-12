using AppCliTools.CliMenu;
using Crawler.MenuCommands;
using ParametersManagement.LibParameters;

namespace Crawler.Menu.Tasks;

// ReSharper disable once ClassNeverInstantiated.Global
public class NewTaskCliMenuCommandFactoryStrategy(IParametersManager parametersManager) : IMenuCommandFactoryStrategy
{
    public CliMenuCommand CreateMenuCommand()
    {
        //ახალი ამოცანის შექმნა
        return new NewTaskCliMenuCommand(parametersManager);
    }
}
