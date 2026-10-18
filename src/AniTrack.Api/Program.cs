using AniTrack.Api.Domain.Abstractions;
using AniTrack.Api.Integrations.AniList;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var baseUrl = builder.Configuration["AniList:BaseUrl"] ?? throw new InvalidOperationException("Configuration 'AniList:BaseUrl' is missing. Add it to appsettings.json.");
builder.Services
      .AddHttpClient<IAnimeProvider, AniListClient>(client =>
      {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
      });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
