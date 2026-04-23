using System.Collections.Generic;
using Crawler.Menu.Batches;
using Crawler.Menu.CrawlerParametersEdit;
using Crawler.Menu.Hosts;
using Crawler.Menu.Schemes;
using Crawler.Menu.Tasks;

namespace Crawler.Menu;

public static class MenuData
{
    public static List<string> MainMenuCommandFactoryStrategyNames { get; } =
    [
        //ძირითადი პარამეტრების რედაქტირება
        nameof(CrawlerParametersEditorCliMenuCommandFactoryStrategy),
        //ჰოსტების რედაქტორი
        nameof(HostListCliMenuCommandFactoryStrategy),
        //სქემების რედაქტორი
        nameof(SchemeListCliMenuCommandFactoryStrategy),
        //პაკეტების რედაქტორი
        nameof(BatchListCliMenuCommandFactoryStrategy),
        //ახალი ამოცანის შექმნა
        nameof(NewTaskCliMenuCommandFactoryStrategy),
        //ამოცანების ჩამონათვალი
        nameof(TasksListFactoryStrategy)
        ////სერვერის პარამეტრების რედაქტირება
        //nameof(SupportToolsServerEditorListCliMenuCommandFactoryStrategy),
        ////ახალი პროექტების შემქმნელი სუბმენიუ
        //nameof(ProjectCreatorSubMenuCliMenuCommandFactoryStrategy),
        ////ახალი პროექტის შექმნა 
        //nameof(CreateNewProjectFactoryStrategy),
        ////პროექტის დაიმპორტება
        //nameof(ImportProjectCliMenuCommandFactoryStrategy),
        ////ყველა პროექტის git-ის სინქრონიზაცია V2
        //nameof(SyncAllProjectsAllGitsCliMenuCommandV2FactoryStrategy),
        ////ყველა პროექტის პაკეტების განახლება
        //nameof(UpdateOutdatedPackagesCliMenuCommandFactoryStrategy),
        ////ყველა ჯგუფების, ყველა სოლუშენის, ყველა პროექტის გასუფთავება
        //nameof(ClearAllGroupsAllSolutionsAllProjectsCliMenuCommandFactoryStrategy),
        ////პროექტების ჯგუფების ჩამონათვალი
        //nameof(ProjectGroupsListFactoryStrategy),
        ////ბოლოს გამოყენებული ბრძანებების ჩამონათვალი
        //nameof(RecentCommandsListFactoryStrategy)
    ];

    //public static List<string> ProjectGroupSubMenuCommandFactoryStrategyNames { get; } =
    //[
    //    //ჯგუფში შემავალი ყველა პროჯეცტის გიტების სინქრონიზაცია
    //    nameof(SyncOneGroupAllProjectsGitsCliMenuCommandV2FactoryStrategy),
    //    //პროექტების ჩამონავთვალი
    //    nameof(ProjectsListFactoryStrategy)
    //];

    //public static List<string> ProjectSubMenuCommandFactoryStrategyNames { get; } =
    //[
    //    //პროექტის წაშლა
    //    nameof(DeleteProjectCliMenuCommandFactoryStrategy),
    //    //პროექტის ექსპორტი
    //    nameof(ExportProjectCliMenuCommandFactoryStrategy),
    //    //პროექტის Visual Studio-ში გახსნა
    //    nameof(OpenByVisualStudioCliMenuCommandFactoryStrategy),
    //    //პროექტის გიტების სინქრონიზაცია
    //    nameof(SyncOneProjectAllGitsWithScaffoldSeedersCliMenuCommandV2FactoryStrategy),
    //    //პროექტის გიტების ჩამონათვალი და მართვა
    //    nameof(GitSubMenuCliMenuCommandFactoryStrategy),
    //    //პროექტის Scaffold Seeder-ის გიტების ჩამონათვალი და მართვა
    //    nameof(GitScaffoldSeederProjectsCliMenuCommandFactoryStrategy),
    //    //დასაშვები ინსტრუმენტების არჩევის საშუალება
    //    nameof(SelectProjectAllowToolsFactoryStrategy),
    //    //დასაშვები ინსტრუმენტები
    //    nameof(ProjectToolsListFactoryStrategy),
    //    //ახალი სერვერის ინფორმაციის შექმნა 
    //    nameof(CreateNewServerInfoFactoryStrategy),
    //    //სერვერების ინფორმაციის ჩამონათვალი და მართვა
    //    nameof(ServerInfosListFactoryStrategy),
    //    //პროექტის პარამეტრების რედაქტირება თანმიმდევრობით
    //    nameof(EditItemAllFieldsInSequenceCliMenuCommandFactoryStrategy),
    //    //პროექტის პარამეტრების სია თავისი რედაქტორებით
    //    nameof(ProjectParametersListFactoryStrategy)
    //];
}
