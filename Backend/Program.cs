using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

const string secretKeyConfigPath = "SymmKey";
if(builder.Configuration[secretKeyConfigPath] is null){throw new InvalidDataException("SymmKey Not Found");}

builder.Services.AddAuthentication(
    (s) => { 
        s.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; 
        s.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }
).AddJwtBearer(s =>
    {
        s.RequireHttpsMetadata = false; //should be false only in dev mode
        s.SaveToken = true;
        s.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.
                            GetBytes(builder.Configuration[secretKeyConfigPath]!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    }
);
builder.Services.AddAuthorization();
// (options =>
// {
//     options.AddPolicy("default", policy => policy.RequireRole([nameof(PersonAccount), nameof(BusinessAccount)]));
// });

//builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddScoped<JWT_TokenService>( args => { return new JWT_TokenService(builder.Configuration[secretKeyConfigPath]!); });

builder.Services.AddScoped<HomePageValidator>();
builder.Services.AddScoped<IHomePageService, HomePageService>();
builder.Services.AddScoped<AccountSignInOutValidator>();
builder.Services.AddScoped<IAccountSignInOutService, AccountSignInOutService>();

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
builder.Services.AddHostedService<AutoUpdateRecentReportsService>();

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
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment()){ app.MapOpenApi(); }
//app.UseHttpsRedirection();

app.UseCors(AllowAllPolicyName);
app.MapControllers();

app.Run();
