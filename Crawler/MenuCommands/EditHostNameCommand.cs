//using System;
//using SystemToolsShared;
//using CliParameters;
//using CliShared;
//using CliShared.DataInput;
//using Crawler.Cruders;
//using DoCrawler.Models;
//using LibCrawlerRepositories;

//namespace Crawler.MenuCommands
//{

//  public sealed class EditHostNameCommand : CliMenuCommand
//  {
//    private readonly ParametersManager _parametersManager;
//    private readonly ICrawlerRepositoryCreatorFabric _crawlerRepositoryCreatorFabric;
//    private readonly string _taskName;

//    public EditHostNameCommand(ParametersManager parametersManager,
//      ICrawlerRepositoryCreatorFabric crawlerRepositoryCreatorFabric, string taskName) : base("Edit Host Name",
//      taskName)
//    {
//      _parametersManager = parametersManager;
//      _crawlerRepositoryCreatorFabric = crawlerRepositoryCreatorFabric;
//      _taskName = taskName;
//    }

//    public override bool Run()
//    {
//      try
//      {

//        CrawlerParameters parameters = (CrawlerParameters)_parametersManager.Parameters;
//        if (parameters == null)
//        {
//          StShared.ConsoleWriteErrorLine("Crawler Parameters not found");
//          return false;
//        }

//        TaskModel task = parameters.GetTask(_taskName);
//        if (task == null)
//        {
//          StShared.ConsoleWriteErrorLine($"Task with name {_taskName} is not found");
//          return false;
//        }

//        //ამოცანის სახელის რედაქტირება
//        //HostCruder hostCruder = new HostCruder(_crawlerRepositoryCreatorFabric);
//        //string currentHostName = task.HostName;
//        //string newHostName = hostCruder.GetNameWithPossibleNewName("Host", currentHostName);

//        //if (task.HostName == newHostName)
//        //  return false; //თუ ცვლილება მართლაც მოითხოვეს

//        //task.HostName = newHostName;

//        //_parametersManager.Save(parameters, $" Task Host Name Changed from {currentHostName} To {newHostName}");

//        MenuAction = EMenuAction.Reload;
//        return true;

//      }
//      catch (DataInputEscapeException)
//      {
//        Console.WriteLine();
//        Console.WriteLine("Escape... ");
//        StShared.Pause();
//      }
//      catch (Exception e)
//      {
//        StShared.WriteException(e);
//      }

//      MenuAction = EMenuAction.Reload;
//      return false;
//    }


//  }


//}

