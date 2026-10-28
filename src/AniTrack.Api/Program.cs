using System.Text.Json.Serialization;
using AniTrack.Api.Domain.Abstractions;
using AniTrack.Api.Infrastructure.ErrorHandling;
using AniTrack.Api.Infrastructure.Persistence;
using AniTrack.Api.Integrations.AniList;
using Microsoft.EntityFrameworkCore;
using AniTrack.Api.Features.PersonalAnimes;
using AniTrack.Features.PersonalAnimes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
      .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<IPersonalAnimesRepository, PersonalAnimesRepository>();
builder.Services.AddScoped<PersonalAnimesService>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

var baseUrl = builder.Configuration["AniList:BaseUrl"] ?? throw new InvalidOperationException("Configuration 'AniList:BaseUrl' is missing. Add it to appsettings.json.");
builder.Services
      .AddHttpClient<IAnimeProvider, AniListClient>(client =>
      {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
      });

var connectionString = builder.Configuration.GetConnectionString("AniTrack") ?? throw new InvalidOperationException(
        "Connection string 'AniTrack' is missing. Set it with 'dotnet user-secrets' in development or via environment variables."); builder.Services

      .AddDbContext<AppDbContext>(client =>
      {
            client.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
      });

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
