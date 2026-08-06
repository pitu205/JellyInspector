using JellyInspector.Application.Interfaces;
using JellyInspector.Application.Models;
using JellyInspector.Domain.Entities;
using JellyInspector.Infrastructure.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace JellyInspector.Web.Components.Pages;

public partial class Settings
{
    [Inject]
    private JellyInspectorDbContext DbContext { get; set; } = default!;

    [Inject]
    private IJellyfinService JellyfinService { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private AppSettings _settings = new()
    {
        Id = 1
    };

    private ServerInfo? _serverInfo;

    private bool _saving;

    protected override async Task OnInitializedAsync()
    {
        _settings =
            await DbContext.AppSettings
                .SingleOrDefaultAsync(x => x.Id == 1)
            ?? new AppSettings
            {
                Id = 1
            };
    }

    private async Task SaveSettingsAsync()
    {
        _saving = true;

        try
        {
            _settings.Id = 1;

            _settings.JellyfinServerUrl =
                NormalizeUrl(_settings.JellyfinServerUrl);

            var existingSettings =
                await DbContext.AppSettings
                    .SingleOrDefaultAsync(x => x.Id == 1);

            if (existingSettings is null)
            {
                DbContext.AppSettings.Add(_settings);
            }
            else
            {
                existingSettings.JellyfinServerUrl =
                    _settings.JellyfinServerUrl;

                existingSettings.JellyfinApiKey =
                    _settings.JellyfinApiKey;

                existingSettings.TmdbApiKey =
                    _settings.TmdbApiKey;
}

            await DbContext.SaveChangesAsync();

            _serverInfo =
                await JellyfinService.GetServerInfoAsync();

            if (_serverInfo is null)
            {
                Snackbar.Add(
                    "Configuración guardada, pero no se pudo conectar con Jellyfin.",
                    Severity.Warning);
            }
            else
            {
                Snackbar.Add(
                    $"Conectado correctamente a {_serverInfo.ServerName} {_serverInfo.Version}.",
                    Severity.Success);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(
                $"No se pudo guardar la configuración: {ex.Message}",
                Severity.Error);
        }
        finally
        {
            _saving = false;
        }
    }

    private static string? NormalizeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        return url.Trim().TrimEnd('/');
    }
}