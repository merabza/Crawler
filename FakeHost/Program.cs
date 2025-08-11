//Created by FakeHostConsoleProgramClassCreator at 8/2/2025 5:16:00 PM

using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

//Add services to the container.

// ReSharper disable once using
var app = builder.Build();

//Configure the HTTP request pipeline.

app.Run();