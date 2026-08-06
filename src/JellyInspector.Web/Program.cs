using JellyInspector.Application.Comparison;
using JellyInspector.Application.Dashboard;
using JellyInspector.Application.Interfaces;
using JellyInspector.Application.Library;
using JellyInspector.Application.Scanning;
using JellyInspector.Infrastructure.Comparison;
using JellyInspector.Infrastructure.Dashboard;
using JellyInspector.Infrastructure.Data;
using JellyInspector.Infrastructure.Jellyfin;
using JellyInspector.Infrastructure.Jellyfin.Clients;
using JellyInspector.Infrastructure.Library;
using JellyInspector.Infrastructure.Repositories;
using JellyInspector.Infrastructure.Scanning;
using JellyInspector.Infrastructure.Services;
using JellyInspector.Infrastructure.Tmdb;
using JellyInspector.Web.Components;
using JellyInspector.Web.Endpoints;
using JellyInspector.Web.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddScoped<ThemeStateService>();

builder.Services.AddDbContext<JellyInspectorDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<IJellyfinService, JellyfinService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient<IJellyfinApiClient, JellyfinApiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<ITmdbClient, TmdbClient>(client =>
{
    client.BaseAddress =
        new Uri("https://api.themoviedb.org/3/");

    client.Timeout =
        TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<IUserClient, UserClient>();
builder.Services.AddScoped<ISeriesClient, SeriesClient>();
builder.Services.AddScoped<ISeasonClient, SeasonClient>();
builder.Services.AddScoped<IEpisodeClient, EpisodeClient>();
builder.Services.AddScoped<ILibraryClient, LibraryClient>();

builder.Services.AddScoped<
    ILibrarySelectionService,
    LibrarySelectionService>();

builder.Services.AddScoped<
    ILastScanService,
    LastScanService>();

builder.Services.AddScoped<
    ISeriesIssueService,
    SeriesIssueService>();

builder.Services.AddScoped<
    ISeriesComparisonService,
    SeriesComparisonService>();

builder.Services.AddScoped<
    IScanSessionRepository,
    ScanSessionRepository>();

builder.Services.AddScoped<
    IScanIssueRepository,
    ScanIssueRepository>();

builder.Services.AddScoped<
    ISeriesRepository,
    SeriesRepository>();

builder.Services.AddScoped<LibraryComparisonService>();

builder.Services.AddScoped<
    IDashboardService,
    DashboardService>();

builder.Services.AddScoped<
    ILibraryService,
    LibraryService>();

builder.Services.AddScoped<
    IScannerService,
    ScannerService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db =
        scope.ServiceProvider
            .GetRequiredService<JellyInspectorDbContext>();

    await db.Database.EnsureCreatedAsync();

    var connection =
        db.Database.GetDbConnection();

    await connection.OpenAsync();

    await EnsureColumnExistsAsync(
        connection,
        "AppSettings",
        "SelectedSeriesLibraryIds",
        "SelectedSeriesLibraryIds TEXT NULL");

    await EnsureColumnExistsAsync(
        connection,
        "Series",
        "TmdbVoteAverage",
        "TmdbVoteAverage REAL NOT NULL DEFAULT 0");

    await EnsureColumnExistsAsync(
        connection,
        "Series",
        "TmdbVoteCount",
        "TmdbVoteCount INTEGER NOT NULL DEFAULT 0");
}

static async Task EnsureColumnExistsAsync(
    System.Data.Common.DbConnection connection,
    string tableName,
    string columnName,
    string columnDefinition)
{
    await using var command =
        connection.CreateCommand();

    command.CommandText =
        $"PRAGMA table_info({tableName});";

    var exists = false;

    await using (var reader =
                 await command.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
        {
            if (string.Equals(
                    reader.GetString(1),
                    columnName,
                    StringComparison.OrdinalIgnoreCase))
            {
                exists = true;
                break;
            }
        }
    }

    if (exists)
    {
        return;
    }

    await using var alter =
        connection.CreateCommand();

    alter.CommandText =
        $"ALTER TABLE {tableName} " +
        $"ADD COLUMN {columnDefinition};";

    await alter.ExecuteNonQueryAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Error",
        createScopeForErrors: true);

    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapJellyfinImageEndpoint();

app.Run();
