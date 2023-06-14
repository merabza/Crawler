dotnet ef database drop --force -c CrawlerDbContext -s Crawler\Crawler.csproj -p CrawlerDbMigration\CrawlerDbMigration.csproj
rem Remove-Item $migrationCsFiles

pause
