using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;
using DoCrawler.Models;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace Crawler.MenuCommands;

public sealed class NewTaskCliMenuCommand : CliMenuCommand
{
    private readonly ParametersManager _parametersManager;

    //ახალი აპლიკაციის ამოცანის შექმნა
    // ReSharper disable once ConvertToPrimaryConstructor
    public NewTaskCliMenuCommand(ParametersManager parametersManager) : base("New Task", EMenuAction.Reload)
    {
        _parametersManager = parametersManager;
    }

    protected override async ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        var parameters = (CrawlerParameters)_parametersManager.Parameters;

        //ამოცანის შექმნის პროცესი დაიწყო
        Console.WriteLine("Create new Task started");

        //ახალი ამოცანის სახელის შეტანა პროგრამაში
        string? newTaskName = Inputer.InputText("New Task Name", null);
        if (string.IsNullOrEmpty(newTaskName))
        {
            return false;
        }

        //გადავამოწმოთ ხომ არ არსებობს იგივე სახელით სხვა ამოცანა.

        if (parameters.Tasks.Keys.Any(a => a == newTaskName))
        {
            StShared.WriteErrorLine(
                $"Task with Name {newTaskName} is already exists. cannot create task with this name. ", true);
            return false;
        }

        //არსებული ინფორმაციის გამოყენებით ახალი ამოცანის შექმნა დაიწყო

        //ახალი ამოცანის შექმნა და ჩამატება ამოცანების სიაში
        parameters.Tasks.Add(newTaskName, new TaskModel());

        //პარამეტრების შენახვა (ცვლილებების გათვალისწინებით)
        await _parametersManager.Save(parameters, "Create New Task Finished");

        //ცვლილებების შენახვა დასრულდა
        //Console.WriteLine("Create new Task Finished");

        //მენიუს შესახებ სტატუსის დაფიქსირება
        //ცვლილებების გამო მენიუს თავიდან ჩატვირთვა და აწყობა
        //რადგან მენიუ თავიდან აეწყობა, საჭიროა მიეთითოს რომელ პროექტში ვიყავით, რომ ისევ იქ დავბრუნდეთ
        //MenuState = new MenuState { RebuildMenu = true, NextMenu = new List<string> { _projectName } };

        //პაუზა იმისათვის, რომ პროცესის მიმდინარეობის შესახებ წაკითხვა მოვასწროთ და მივხვდეთ, რომ პროცესი დასრულდა
        //StShared.Pause();

        //ყველაფერი კარგად დასრულდა
        return true;
    }
}
