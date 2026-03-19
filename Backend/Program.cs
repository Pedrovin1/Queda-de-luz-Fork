using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using Dapper;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddScoped<IHomePageService, HomePageService>();

//To Avoid Wasting Time for a prototype
const string AllowAllPolicyName = "AllowAll";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowAllPolicyName, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

//Database Configs
const string DatabaseDirectory = @".\Database\";
const string DatabaseFile = @"BlackoutMap.db";
const string connectionString= @"Data Source=" + DatabaseDirectory + DatabaseFile;
const string DBCreationSqlPath = DatabaseDirectory + "BlackoutMapDBCreate.sql";

builder.Services.AddSingleton<IBlackoutMapConnectionFactory>(_ => new BlackoutMapConnectionFactory(connectionString));

if (!Directory.Exists(DatabaseDirectory) || !File.Exists(DatabaseDirectory + DatabaseFile))
{
    Directory.CreateDirectory(DatabaseDirectory);
    File.Create(DatabaseDirectory + DatabaseFile).Close();

    var connFactory = new BlackoutMapConnectionFactory(connectionString);
    
    using var DB_Generator = await connFactory.CreateConnectionAsync();
    string sqlCreate = await File.ReadAllTextAsync(DBCreationSqlPath);

    await DB_Generator.ExecuteAsync(sqlCreate);
    await DB_Generator.CloseAsync();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment()){ app.MapOpenApi(); }
//app.UseHttpsRedirection();

app.UseCors(AllowAllPolicyName);
app.MapControllers();

app.Run();
