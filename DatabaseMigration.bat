rem -c CrawlerDbContext 
rem start with "Initial"
dotnet ef migrations add "Initial" -c CrawlerDbContext -s Crawler\Crawler.csproj -p CrawlerDbMigration\CrawlerDbMigration.csproj
dotnet ef database update -c CrawlerDbContext -s Crawler\Crawler.csproj -p CrawlerDbMigration\CrawlerDbMigration.csproj
pause
