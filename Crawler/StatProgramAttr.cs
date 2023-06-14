using SystemToolsShared;

namespace Crawler;

public static class StatProgramAttr
{
    public static void SetAttr()
    {
        ProgramAttributes.Instance.SetAttribute("AppName", "Crawler");
    }
}