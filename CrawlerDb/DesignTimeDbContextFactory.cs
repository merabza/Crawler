using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CrawlerDb;

//ეს არის ზოგადი კლასი მიგრაციასთან სამუშაოდ
//თუმცა მე გადავაკეთე ისე, რომ კონსტრუქტორი ღებულობს ინფორმაციას:
//1. სად ეძებოს პარამეტრების ფაილი, ==\== ამის გადაცემა გადავიფიქრე, რადგან კოდი მიმდინარე ფოლდერში იყურება
//და უფრო სწორია, თუ მიგრაციის პროცესს იმ ფოლდერიდან გავუშვებთ, რომელშიც პარამეტრებია ჩაწერილი.
//მიგრაციის და ბაზის კონტექსტის შესახებ ინფორმაციის გადაწოდება კი მიგრაციის ბრძანებისათვის შესაძლებელია და სკრიპტიდან მოხდება
//2. რა ჰქვია პარამეტრების ფაილს
//3. რომელ პარამეტრში წერია ბაზასთან დასაკავშირებელი სტრიქონი
//ბაზასთან დაკავშირების სტრიქონის გადმოწოდება არასწორია, რადგან მომიწევდა ამ სტრიქონის გამშვები პროექტის კოდში ჩაშენება.
//რაც უსაფრთხოების თვალსაზრისით არასწორია
public /*open*/ class DesignTimeDbContextFactory<T> : IDesignTimeDbContextFactory<T> where T : DbContext
{
    private readonly string _assemblyName;
    private readonly string _connectionParamName;
    private readonly string? _parametersJsonFileName;

    protected DesignTimeDbContextFactory(string assemblyName, string connectionParamName,
        string? parametersJsonFileName = null)
    {
        _assemblyName = assemblyName;
        _connectionParamName = connectionParamName;
        _parametersJsonFileName = parametersJsonFileName;
        Console.WriteLine($"DesignTimeDbContextFactory assemblyName = {assemblyName}");
        Console.WriteLine($"DesignTimeDbContextFactory connectionParamName = {connectionParamName}");
        Console.WriteLine($"DesignTimeDbContextFactory parametersJsonFileName = {parametersJsonFileName}");
    }

    public T CreateDbContext(string[] args)
    {
        Console.WriteLine($"Pass 1... CurrentDirectory is {Directory.GetCurrentDirectory()}");

        //თუ პარამეტრების json ფაილის სახელი პირდაპირ არ არის გადმოცემული, ვიყენებთ სტანდარტულ სახელს appsettings.json
        var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(_parametersJsonFileName ?? "appsettings.json", false, true)
            //.AddEncryptedJsonFile(Path.Combine(pathToContentRoot, "appsettingsEncoded.json"), optional: false, reloadOnChange: true, Key,
            //  Path.Combine(pathToContentRoot, "appsetenkeys.json"))

            //.AddUserSecrets<TSt>()
            //.AddEnvironmentVariables()
            .Build();
        //Console.WriteLine("Pass 2...");
        var connectionString = configuration[_connectionParamName];
        //Console.WriteLine("Pass 3...");

        var builder = new DbContextOptionsBuilder<T>();
        //Console.WriteLine("Pass 4...");
        builder.UseSqlServer(connectionString, b => b.MigrationsAssembly(_assemblyName));
        //Console.WriteLine("Pass 5...");
        var dbContext = Activator.CreateInstance(typeof(T), builder.Options, true);

        if (dbContext is null)
            throw new Exception("dbContext does not created");
        //Console.WriteLine("Pass 6...");
        return (T)dbContext;
    }
}