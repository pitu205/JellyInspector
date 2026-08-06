using JellyInspector.Domain.Entities;
using JellyInspector.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JellyInspector.Web.Services;

public sealed class ThemeStateService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ThemeStateService(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public bool IsDarkMode { get; private set; }

    public event Action? Changed;

    public async Task LoadAsync()
    {
        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<JellyInspectorDbContext>();

        var settings =
            await db.AppSettings
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.Id == 1);

        IsDarkMode =
            settings?.DarkMode ?? false;

        Changed?.Invoke();
    }

    public async Task ToggleAsync()
    {
        await SetAsync(!IsDarkMode);
    }

    public async Task SetAsync(
        bool isDarkMode)
    {
        IsDarkMode = isDarkMode;

        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<JellyInspectorDbContext>();

        var settings =
            await db.AppSettings
                .SingleOrDefaultAsync(item => item.Id == 1);

        if (settings is null)
        {
            settings = new AppSettings
            {
                Id = 1,
                DarkMode = isDarkMode
            };

            db.AppSettings.Add(settings);
        }
        else
        {
            settings.DarkMode = isDarkMode;
        }

        await db.SaveChangesAsync();

        Changed?.Invoke();
    }
}
