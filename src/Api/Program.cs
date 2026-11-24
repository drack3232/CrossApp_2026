using Api.Endpoints;
using Api.Infrastructure;
using Api.Validators;
using Core.Abstractions; // або using Core;
using FluentValidation;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddValidatorsFromAssemblyContaining<SaveOrderRequestValidator>();

var rawConnectionString = builder.Configuration.GetConnectionString("AppDb") ?? "Data Source=data/app.db";
var csb = new SqliteConnectionStringBuilder(rawConnectionString);
if (!Path.IsPathRooted(csb.DataSource))
{
    var fullPath = Path.Combine(builder.Environment.ContentRootPath, csb.DataSource);
    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
    csb.DataSource = fullPath;
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(csb.ConnectionString));
builder.Services.AddScoped<IOrderStore, EfOrderStore>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    if (app.Environment.IsDevelopment())
    {
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
    }
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/health", () => TypedResults.Ok(new { status = "ok" }));
app.MapOrders();

app.Run();

public partial class Program { }