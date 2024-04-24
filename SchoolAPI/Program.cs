using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using SchoolAPI.DAL;
using SchoolAPI.Models;
using SchoolAPI.Service;
using System.Diagnostics.Metrics;
using System.Security.Claims;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        string? otelConnectionString;
        string? authSecretKey;
        string? kustoAppId;
        string? kustoAppKey;
        // check for development environment
        if (builder.Environment.IsProduction())
        {
            otelConnectionString = Environment.GetEnvironmentVariable("APPINSIGHTS_CONNECTIONSTRING");
            authSecretKey = Environment.GetEnvironmentVariable("API_SECRETFORKEYGENERATION");
            kustoAppId = Environment.GetEnvironmentVariable("kusto_APPLICATION_ID");
            kustoAppKey = Environment.GetEnvironmentVariable("kusto_APPLICATION_KEY");
        }
        else
        {
            otelConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
            authSecretKey = builder.Configuration["Authentication:SecretForKey"];
            kustoAppId = builder.Configuration["kusto:AppId"];
            kustoAppKey = builder.Configuration["kusto:AppSecret"];
        }

        if (string.IsNullOrEmpty(otelConnectionString) || string.IsNullOrEmpty(authSecretKey) || string.IsNullOrEmpty(kustoAppId) || string.IsNullOrEmpty(kustoAppKey))
        {
            throw new Exception("Environment variables not set");
        }

        builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new()
                    {
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.ASCII.GetBytes(authSecretKey))
                    };
                }
            );
        var otel = builder.Services.AddOpenTelemetry();


        otel.UseAzureMonitor(options => {
            options.ConnectionString = otelConnectionString;
        });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("UserRolePolicy", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim(ClaimTypes.Role, "Teacher");
            });
        });

        KustoAuthDetails kustoAuth = new KustoAuthDetails
        {
            AppId = kustoAppId,
            AppKey = kustoAppKey
        };

        IDataClient dataClient = new KustoDataClient(builder.Configuration, kustoAuth);
        SchoolAPIService apiService = new SchoolAPIService(dataClient, builder.Configuration);

        builder.Services.AddSingleton<KustoAuthDetails>(kustoAuth);
        builder.Services.AddSingleton<IDataClient>(dataClient);
        builder.Services.AddSingleton<SchoolAPIService>(apiService);
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }


        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}