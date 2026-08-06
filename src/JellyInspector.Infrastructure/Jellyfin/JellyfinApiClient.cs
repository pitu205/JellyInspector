using System.Net.Http.Json;
using JellyInspector.Domain.Entities;
using JellyInspector.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JellyInspector.Infrastructure.Jellyfin;

public class JellyfinApiClient : IJellyfinApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JellyInspectorDbContext _dbContext;
    private readonly ILogger<JellyfinApiClient> _logger;

    public JellyfinApiClient(
        HttpClient httpClient,
        JellyInspectorDbContext dbContext,
        ILogger<JellyfinApiClient> logger)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await SendAsync(
                HttpMethod.Get,
                endpoint,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody =
                    await response.Content.ReadAsStringAsync(
                        cancellationToken);

                _logger.LogWarning(
                    "Jellyfin devolvió {StatusCode} al consultar {Endpoint}. " +
                    "Respuesta: {ResponseBody}",
                    (int)response.StatusCode,
                    endpoint,
                    responseBody);

                return default;
            }

            return await response.Content.ReadFromJsonAsync<T>(
                cancellationToken: cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Error HTTP al consultar Jellyfin: {Endpoint}",
                endpoint);

            return default;
        }
        catch (TaskCanceledException ex)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                ex,
                "Tiempo de espera agotado al consultar Jellyfin: {Endpoint}",
                endpoint);

            return default;
        }
    }

    public async Task<byte[]?> GetBytesAsync(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await SendAsync(
                HttpMethod.Get,
                endpoint,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Jellyfin devolvió {StatusCode} al descargar {Endpoint}",
                    (int)response.StatusCode,
                    endpoint);

                return null;
            }

            return await response.Content.ReadAsByteArrayAsync(
                cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Error descargando datos desde Jellyfin: {Endpoint}",
                endpoint);

            return null;
        }
        catch (TaskCanceledException ex)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                ex,
                "Tiempo de espera agotado descargando desde Jellyfin: {Endpoint}",
                endpoint);

            return null;
        }
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        var settings = await GetSettingsAsync(cancellationToken);

        if (!HasValidSettings(settings))
        {
            throw new InvalidOperationException(
                "La configuración de Jellyfin no está completa.");
        }

        var requestUri = BuildRequestUri(
            settings!,
            endpoint);

        var request = new HttpRequestMessage(
            method,
            requestUri);

        request.Headers.Add(
            "X-Emby-Token",
            settings!.JellyfinApiKey);

        _logger.LogDebug(
            "Enviando petición {Method} a Jellyfin: {RequestUri}",
            method,
            requestUri);

        try
        {
            return await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
        }
        finally
        {
            request.Dispose();
        }
    }

    private async Task<AppSettings?> GetSettingsAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.AppSettings
            .AsNoTracking()
            .SingleOrDefaultAsync(
                settings => settings.Id == 1,
                cancellationToken);
    }

    private static bool HasValidSettings(
        AppSettings? settings)
    {
        return settings is not null &&
               !string.IsNullOrWhiteSpace(
                   settings.JellyfinServerUrl) &&
               !string.IsNullOrWhiteSpace(
                   settings.JellyfinApiKey);
    }

    private static string BuildRequestUri(
        AppSettings settings,
        string endpoint)
    {
        var baseUrl = settings.JellyfinServerUrl!
            .Trim()
            .TrimEnd('/');

        var normalizedEndpoint = endpoint
            .Trim()
            .TrimStart('/');

        return $"{baseUrl}/{normalizedEndpoint}";
    }
}