using System;
using CliMenu;
using CliParameters;

namespace Crawler.MenuCommands;

public sealed class HostSubMenuCommand : CliMenuCommand
{
    private readonly Cruder _cruder;

    public HostSubMenuCommand(Cruder cruder, string hostName, string parentMenuName, bool nameIsStatus = false) : base(
        hostName, parentMenuName, false, EStatusView.Brackets, nameIsStatus)
    {
        _cruder = cruder;
    }

    protected override void RunAction()
    {
        MenuAction = EMenuAction.LoadSubMenu;
    }

    public override CliMenuSet GetSubmenu()
    {
        if (Name is null)
            throw new Exception("Name is null");
        return _cruder.GetItemMenu(Name);
    }


    protected override string GetStatus()
    {
        if (Name is null)
            throw new Exception("Name is null");
        return _cruder.GetStatusFor(Name) ?? "";
    }
}