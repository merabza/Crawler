# Crawler → CrawlerConsole + CrawlerService გაყოფის გეგმა

სტატუსი: **დასამტკიცებელი დრაფტი** · თარიღი: 2026-06-09

## 1. მიზანი და შეთანხმებული მოცულობა

მონოლითური `Crawler` კონსოლ-აპლიკაცია იყოფა ProcessorWorker-ის ნიმუშით:

- **CrawlerConsole** — მენიუ და მომხმარებელთან ურთიერთობა. **ინარჩუნებს ბაზასთან წვდომას** და ყველა მონაცემის რედაქტირებას (Hosts / Schemes / Batches / HostByBatch Cruder-ები, Tasks/StartPoints პარამეტრების JSON-ში).
- **CrawlerService** — **მხოლოდ ქრაულინგის შესრულება**. ASP.NET Core Web API + background worker, Windows Service-ად გაფორმებადი. გრძელვადიან crawl-ს უშვებს `ReCounter`-ით და პროგრესს აბრუნებს SignalR-ით.
- **CrawlerServiceShared** — გაზიარებული კონტრაქტები (API client, routes, enums, errors), ორივე მხარე იყენებს.

გადადის სერვისში მხოლოდ 3 ოპერაცია (მიმდინარე seam):

| მენიუ-ბრძანება | ამჟამად | გაყოფის შემდეგ |
|---|---|---|
| `BatchTaskCliMenuCommand` „Run this batch" | `CrawlerRunnerToolAction(batch)` in-process | `POST /crawler/runbatch` |
| `TaskCliMenuCommand` „Run this task" | `CrawlerRunnerToolAction(task)` in-process | `POST /crawler/runtask` |
| `TestOnePageCliMenuCommand` „Test One Page" | `OnePageCrawlerRunnerToolAction(url)` in-process | `POST /crawler/testonepage` |

ყველა დანარჩენი (მენიუ, Cruder-ები, პარამეტრების რედაქტირება, DB connection შემოწმება) რჩება CrawlerConsole-ში უცვლელად.

## 2. სამიზნე ტოპოლოგია (3 workspace, PW-ის კონვენციით)

```
D:\1WorkDotnet\
├── CrawlerServiceShared\               (ახალი repo — გაზიარებული კონტრაქტები)
│   └── CrawlerServiceShared\
│       ├── <shared libs: SystemTools.ReCounterContracts ...>
│       ├── CrawlerServiceShared\                 (NuGet wrapper)
│       └── CrawlerServiceShared.Contracts\       (ApiClient, routes, enums, errors)
│
├── CrawlerConsole\                     (ახალი workspace)
│   ├── AppCliTools\  ParametersManagement\  SystemTools\  ToolsManagement\
│   │   DatabaseTools\  ConnectionTools\  CrawlerServiceShared\   (sibling shared libs)
│   ├── Crawler\  ← crawler-core (CrawlerDb, LibCrawlerRepositories, DoCrawler, RobotsTxt)
│   └── CrawlerConsole\                  (ახალი repo — main)
│       ├── CrawlerConsole\              (Exe: მენიუ + Cruder-ები)
│       ├── DoCrawlerConsole\            (ToolCommands → API client)
│       └── CrawlerConsoleParametersData\(პარამეტრები + TaskModel)
│
└── CrawlerService\                     (ახალი workspace)
    ├── SystemTools\  WebSystemTools\  ParametersManagement\  ToolsManagement\
    │   DatabaseTools\  CrawlerServiceShared\          (sibling shared libs)
    ├── Crawler\  ← crawler-core (CrawlerDb, LibCrawlerRepositories, DoCrawler, RobotsTxt, CrawlerDbMigration)
    └── CrawlerService\                  (ახალი repo — main)
        ├── CrawlerService\              (Web Exe: Program.cs + DI + Windows Service)
        ├── CrawlerServiceApi\           (Minimal API endpoints + MediatR)
        └── CrawlerServiceReCounters\    (CrawlerReCounter — ახვევს crawl-ძრავას)
```

## 3. ❓Decision A — crawler-core კოდის გაზიარება (დასადასტურებელი)

`CrawlerDb`, `LibCrawlerRepositories`, `DoCrawler`, `RobotsTxt` სჭირდება **ორივეს** (console — Cruder-ებისთვის; service — crawl-ისთვის). PW-ში ეს კოდი service-ში მხოლოდ იყო — ჩვენთან განსხვავებაა, რადგან console ინარჩუნებს ბაზას.

**რეკომენდაცია (A1):** არსებული `Crawler` repo გადაკეთდეს „crawler-core გაზიარებულ ბიბლიოთეკად" — მისგან წაიშლება მონოლითური `Crawler` (Exe) პროექტი (მისი მენიუ/Cruder-ები გადავა ახალ `CrawlerConsole`-ში), დარჩება `CrawlerDb`, `LibCrawlerRepositories`, `DoCrawler`, `RobotsTxt`, `CrawlerDbMigration`, `Crawler.Tests`, `FakeHost`. ორივე ახალი workspace მას sibling-ად დააკლონებს და მიაშურებს `..\Crawler\<proj>`-ით. ერთი წყარო, დუბლირების გარეშე.

**ალტერნატივა (A2):** crawler-core პროექტები დაკოპირდეს თითო workspace-ში ცალკე. მარტივი, მაგრამ ორმაგი წყარო / drift-ის რისკი.

→ **საჭიროა შენი არჩევანი: A1 (რეკომენდებული) თუ A2.**

## 4. ❓Decision B — crawl-კონფიგურაციის მფლობელობა (დასადასტურებელი)

crawl-ის შესასრულებლად სერვისს სჭირდება: DB connection string, `Alphabet`, `ExtraSymbols`, `LoadPagesMaxCount`, `Punctuations`, `SmartSchemas` (regex-ების ასაგებად). ამჟამად ეს ყველაფერი `CrawlerParameters`-შია (console-ის JSON).

**რეკომენდაცია:**
- **crawl-execution პარამეტრები → CrawlerService `appsettings.json`** (DB connection + parsing settings). PW-ის სტილი.
- **per-run შემავალი მონაცემები → request payload** console-იდან: batch-ის სახელი/ID, task-ის StartPoints, test URL, `countType`.
- console-ის `parameters.json` ინარჩუნებს: ApiClient-ს (სერვისთან წვდომა), Tasks/StartPoints-ს (მენიუსთვის), + DB connection-ს (Cruder-ებისთვის).

→ **საჭიროა დადასტურება: parsing-პარამეტრები სერვისის appsettings-ში გადავიდეს, თუ console აგზავნოს request-ში.**

## 5. CrawlerServiceShared (კონტრაქტები)

ProcessorWorkerServiceShared-ის ზუსტი ანალოგი:

- **`CrawlerServiceShared.Contracts`** (ref: `SystemTools.ReCounterContracts`):
  - `CrawlerServiceApiClient : ReCounterApiClient` — მეთოდები: `RunBatch(...)`, `RunTask(...)`, `TestOnePage(...)`, `IsRunning(...)`. cancel/status/progress base-კლასიდან მოდის.
  - `V1/Routes/CrawlerServiceApiRoutes.cs` — `ApiBase = api/v1`, `CrawlerRoute` ჯგუფი + route-კონსტანტები.
  - `ECountType` (Test/Full) — ან არსებული მნიშვნელობის enum.
  - `Errors/CrawlerServiceErrors.cs`.
  - per-run DTO-ები (თუ Decision B → payload): `RunTaskRequest { StartPoints[], ... }` და ა.შ.
- **`CrawlerServiceShared`** — NuGet wrapper (refs `.Contracts`).

## 6. CrawlerService (სერვისი)

PW-service-ის სტრუქტურა, crawler-domain-ზე მორგებული.

| პროექტი | SDK | როლი |
|---|---|---|
| `CrawlerService` | `Microsoft.NET.Sdk.Web` | Program.cs: `WebApplication.CreateBuilder` + `UseWindowsServiceOnWindows`; DI chain; appsettings; Kestrel; ApiKey |
| `CrawlerServiceApi` | `Microsoft.NET.Sdk` | `Endpoints/V1/CrawlerEndpoints.cs` (Minimal API) + `CommandRequests/` + `Handlers/` (MediatR) |
| `CrawlerServiceReCounters` | `Microsoft.NET.Sdk` | `CrawlerReCounter : DatabaseReCounter` — ახვევს `BatchPartRunner`/crawl-ლოგიკას, პროგრესს `IProgressDataManager`-ით |

**references:** crawler-core (`CrawlerDb`, `LibCrawlerRepositories`, `DoCrawler`, `RobotsTxt`), `WebSystemTools.*` (MediatorTools, ApiExceptionHandler, ApiKeyIdentity, SignalRRecounterMessages, ConfigurationEncrypt, SerilogLogger, SwaggerTools, WindowsServiceTools, TestToolsApi), `SystemTools.*` (ReCounterAbstraction, MediatRMessagingAbstractions, ReCounterContracts, ...), `ParametersManagement.*`, `ToolsManagement.ApiClientsManagement`, `DatabaseTools.*`, `CrawlerServiceShared.Contracts`.

**ნაკადი** (PW-ის იდენტური): `POST endpoint` → MediatR command → handler → `IReCounterBackgroundTaskQueue.QueueBackgroundWorkItem(...)` → `CrawlerReCounter.Recount()` → `BatchPartRunner.RunBatchPart()` → პროგრესი SignalR-ით → დაუყოვნებლივ ბრუნდება `true` კლიენტს.

**cancel/status/progress** — მზად მოდის `WebSystemTools.SignalRRecounterMessages`-იდან (CancelCurrentProcess / CurrentProcessStatus / messages hub). ცალკე წერა საჭირო არ არის.

## 7. CrawlerConsole (კონსოლი)

PW-console-ის სტრუქტურა + შენარჩუნებული DB/Cruder-ები.

| პროექტი | SDK | როლი |
|---|---|---|
| `CrawlerConsole` | `Exe` | Program.cs + `CrawlerConsoleMenuBuilder` + DI; **მენიუ + Cruder-ები + FieldEditors** (გადმოტანილი ძველი `Crawler`-იდან); DB connection შენარჩუნებული |
| `DoCrawlerConsole` | lib | `ApiClientToolCommand` base + ToolCommands, რომლებიც `CrawlerServiceApiClient`-ით ეძახიან სერვისს (RunBatch/RunTask/TestOnePage + ProcessMonitoring) |
| `CrawlerConsoleParametersData` | lib | `CrawlerConsoleParameters` (ApiClients, Tasks, DB connection, parsing settings რჩება console-ში მენიუსთვის) + `TaskModel` |

**references:** crawler-core (`CrawlerDb`, `LibCrawlerRepositories`, `DoCrawler`, `RobotsTxt`), `AppCliTools.*`, `SystemTools.*` (+ `ReCounterContracts` ApiClient base-ისთვის), `ParametersManagement.*`, `DatabaseTools.*`, `ToolsManagement.*`, `CrawlerServiceShared.Contracts`.

**3 seam-ბრძანების ცვლილება** (მაგ. `BatchTaskCliMenuCommand`):
- **იყო:** `new CrawlerRunnerToolAction(...)` → `new CrawlerRunner(...).Run()` (in-process, Console output, Pause).
- **გახდება:** `CreateClient()` → `crawlerServiceApiClient.RunBatch(batch, countType)` → შემდეგ `ProcessMonitoring` (SignalR-პროგრესი ცოცხლად). `Cruders/BatchCruder.cs`-ში „Run this batch" item რჩება, მაგრამ API-ს ეძახის.

## 8. crawler-core მინიმალური ცვლილება — პროგრესის აბსტრაქცია

`BatchPartRunner`/`ToolActions` ამჟამად `Console.WriteLine`-ით წერენ პროგრესს. სერვისს სჭირდება SignalR.

**რეკომენდაცია:** `DoCrawler`-ში დაემატოს მცირე `ICrawlerProgress` (Message / SetLength / Increment), default = console impl (console-ის ქცევა უცვლელი). `BatchPartRunner`-ის ~რამდენიმე პროგრეს-წერტილი ამ ინტერფейსზე გადავა. სერვისის `CrawlerReCounter` მისცემს impl-ს, რომელიც `IProgressDataManager`-ში გადაამისამართებს. ეს ერთადერთი შემხები ცვლილებაა გაზიარებულ ძრავაში.

## 9. გაზიარებული ბიბლიოთეკები — დასაკოპირებელი (წყაროებით)

CrawlerService workspace-ს სჭირდება **WebSystemTools**, რომელიც Crawler workspace-ში არ არის → წყარო: `D:\1WorkDotnet\ProcessorWorkerService\WebSystemTools\`.

- **CrawlerConsole workspace:** `AppCliTools`, `ParametersManagement`, `SystemTools`, `ToolsManagement`, `DatabaseTools`, `ConnectionTools` (← `D:\1WorkDotnet\Crawler\`-დან) + `Crawler` (crawler-core) + `CrawlerServiceShared`.
- **CrawlerService workspace:** `SystemTools`, `ParametersManagement`, `ToolsManagement`, `DatabaseTools` (← `D:\1WorkDotnet\Crawler\`), `WebSystemTools` (← `ProcessorWorkerService`), + `Crawler` (crawler-core) + `CrawlerServiceShared`.
- `Directory.Build.props` / `Directory.Packages.props` თითო main repo-ში (PW-ის მსგავსად, საჭირო პაკეტ-ვერსიებით: Figgle.Fonts, MediatR, FluentValidation, EF Design და ა.შ.).

## 10. API კონტრაქტი (პირველი ვერსია)

```
POST  api/v1/crawler/runbatch?batchName={name}&countType={Test|Full}
POST  api/v1/crawler/runtask         (body: RunTaskRequest | query taskName)
POST  api/v1/crawler/testonepage     (body/query: taskName, url)
GET   api/v1/crawler/isrunning
+ messages hub / cancel / status  ← WebSystemTools.SignalRRecounterMessages-იდან მზამზარეული
```
ApiKey ავთენტიფიკაცია (query `?apiKey=`+remote IP), PW-ის იდენტური.

## 11. ფაზები (verify-ნაბიჯებით)

1. **CrawlerServiceShared** — repo + Contracts (ApiClient, routes, enums, errors). ✅ build.
2. **crawler-core გამზადება** (Decision A1) — Crawler repo-დან `Crawler` Exe-ს ამოღება; `ICrawlerProgress` დამატება. ✅ crawler-core build.
3. **CrawlerService workspace** — shared libs კოპირება (+WebSystemTools), 3 პროექტი, Program.cs, endpoints, MediatR handlers, `CrawlerReCounter`. ✅ `dotnet build`, ✅ სერვისი ეშვება `--console`-ით, Swagger ეხსნება.
4. **CrawlerConsole workspace** — shared libs, 3 პროექტი; მენიუ/Cruder-ების გადმოტანა; 3 seam-ბრძანების API-ზე გადაყვანა; ApiClient + ProcessMonitoring. ✅ build, ✅ მენიუ მუშაობს, Cruder-ები ბაზას ხედავენ.
5. **End-to-end** — console-იდან RunBatch → service crawl-ს ასრულებს → პროგრესი ცოცხლად console-ში. ✅ ერთი რეალური batch.

## 12. ღია საკითხები (დასადასტურებელი სანამ დავიწყებ)

1. **Decision A** — A1 (Crawler repo → crawler-core, რეკომ.) თუ A2 (დუბლირება)?
2. **Decision B** — parsing-პარამეტრები სერვისის appsettings-ში თუ console-ის request-ში?
3. **`countType` / Test vs Full** — გადმოგვაქვს არსებული `ECountType` თუ ახალი enum?
4. **git** — თითო ახალ workspace-ში ცალკე `git init` + GitHub remote (PW-ის მსგავსად `merabza/CrawlerConsole`, `merabza/CrawlerService`, `merabza/CrawlerServiceShared`)? თუ ჯერ მხოლოდ ლოკალური?
5. **gRPC vs HTTP** — ვტოვებ HTTP+SignalR-ს (PW-ის იდენტური).

## 13. მოცულობის გარეთ

- Hosts/Schemes/Batches CRUD-ის API-ზე გადატანა (ეს იქნებოდა „სრული template-პარიტეტი", უარყოფილი).
- ახალი crawl-ფუნქციონალი; ქცევის ცვლილება.
- DB სქემის ცვლილება.
