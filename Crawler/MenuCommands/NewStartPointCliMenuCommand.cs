using System;
using System.Linq;
using CliMenu;
using DoCrawler.Models;
using LibDataInput;
using LibParameters;
using SystemToolsShared;

namespace Crawler.MenuCommands;

public sealed class NewStartPointCliMenuCommand : CliMenuCommand
{
    private readonly ParametersManager _parametersManager;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public NewStartPointCliMenuCommand(ParametersManager parametersManager, string taskName) : base("New Start Point")
    {
        _parametersManager = parametersManager;
        _taskName = taskName;
    }

    protected override void RunAction()
    {
        try
        {
            var parameters = (CrawlerParameters)_parametersManager.Parameters;

            var task = parameters.GetTask(_taskName);

            if (task == null)
            {
                StShared.WriteErrorLine($"Task with name {_taskName} not found", true);
                return;
            }

            //ამოცანის შექმნის პროცესი დაიწყო
            Console.WriteLine("Create new Start Point started");

            //ახალი ამოცანის სახელის შეტანა პროგრამაში
            var newStartPoint = Inputer.InputText("New Start Point", null);
            if (string.IsNullOrWhiteSpace(newStartPoint))
                return;
            //გადავამოწმოთ ხომ არ არსებობს იგივე სახელით სხვა ამოცანა.

            if (task.StartPoints.Any(a => a == newStartPoint))
            {
                StShared.WriteErrorLine(
                    $"Start Point with Name {newStartPoint} is already exists. cannot create Start Point with this name. ",
                    true);
                return;
            }

            //ახალი ამოცანის შექმნა და ჩამატება ამოცანების სიაში
            task.StartPoints.Add(newStartPoint);

            //პარამეტრების შენახვა (ცვლილებების გათვალისწინებით)
            _parametersManager.Save(parameters, "Create New Task Finished");

            //ცვლილებების შენახვა დასრულდა
            //Console.WriteLine("Create new Task Finished");

            //მენიუს შესახებ სტატუსის დაფიქსირება
            //ცვლილებების გამო მენიუს თავიდან ჩატვირთვა და აწყობა
            //რადგან მენიუ თავიდან აეწყობა, საჭიროა მიეთითოს რომელ პროექტში ვიყავით, რომ ისევ იქ დავბრუნდეთ
            //MenuState = new MenuState { RebuildMenu = true, NextMenu = new List<string> { _projectName } };
            MenuAction = EMenuAction.Reload;

            //პაუზა იმისათვის, რომ პროცესის მიმდინარეობის შესახებ წაკითხვა მოვასწროთ და მივხვდეთ, რომ პროცესი დასრულდა
            //StShared.Pause();

            //ყველაფერი კარგად დასრულდა
        }
        catch (DataInputEscapeException)
        {
            Console.WriteLine();
            Console.WriteLine("Escape... ");
            StShared.Pause();
        }
        catch (Exception e)
        {
            StShared.WriteException(e, true);
        }
    }
}