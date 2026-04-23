using AppCliTools.CliMenu;
using Crawler.MenuCommands;
using ParametersManagement.LibParameters;

namespace Crawler.Menu.Tasks;

// ReSharper disable once ClassNeverInstantiated.Global
public class NewTaskCliMenuCommandFactoryStrategy : IMenuCommandFactoryStrategy
{
    private readonly ParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public NewTaskCliMenuCommandFactoryStrategy(IParametersManager parametersManager)
    {
        _parametersManager = (ParametersManager)parametersManager;
    }

    public CliMenuCommand CreateMenuCommand()
    {
        //ახალი ამოცანის შექმნა
        return new NewTaskCliMenuCommand(_parametersManager);
    }
}
