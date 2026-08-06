using System.Net.Http.Json;
using JellyInspector.Application.Interfaces;
using JellyInspector.Application.Library;
using JellyInspector.Application.Models;
using JellyInspector.Domain.Entities;
using JellyInspector.Infrastructure.Data;
using JellyInspector.Infrastructure.Jellyfin.Clients;
using Microsoft.EntityFrameworkCore;

namespace JellyInspector.Infrastructure.Services;

public class JellyfinService : IJellyfinService
{
    private readonly HttpClient _httpClient;
    private readonly JellyInspectorDbContext _dbContext;
    private readonly ISeriesClient _seriesClient;
    private readonly ILibrarySelectionService _librarySelectionService;

    public JellyfinService(
        HttpClient httpClient,
        JellyInspectorDbContext dbContext,
        ISeriesClient seriesClient,
        ILibrarySelectionService librarySelectionService)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
        _seriesClient = seriesClient;
        _librarySelectionService = librarySelectionService;
    }

    public async Task<bool> TestConnectionAsync()
    {
        return await GetServerInfoAsync() is not null;
    }

    public async Task<ServerInfo?> GetServerInfoAsync()
    {
        try
        {
            var settings = await GetSettingsAsync();

            if (!HasValidJellyfinSettings(settings))
            {
                return null;
            }

            using var request = CreateRequest(
                settings!,
                "/System/Info");

            using var response =
                await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result =
                await response.Content
                    .ReadFromJsonAsync<JellyfinServerInfoResponse>();

            if (result is null)
            {
                return null;
            }

            return new ServerInfo
            {
                ServerName = result.ServerName ?? string.Empty,
                Version = result.Version ?? string.Empty,
                Id = result.Id ?? string.Empty
            };
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<LibraryInfo>> GetLibrariesAsync()
    {
        try
        {
            var settings = await GetSettingsAsync();

            if (!HasValidJellyfinSettings(settings))
            {
                return [];
            }

            using var request = CreateRequest(
                settings!,
                "/Library/VirtualFolders");

            using var response =
                await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return [];
            }

            var result =
                await response.Content
                    .ReadFromJsonAsync<List<JellyfinLibraryResponse>>();

            if (result is null)
            {
                return [];
            }

            return result
                .Select(library => new LibraryInfo
                {
                    Id = library.ItemId ?? string.Empty,
                    Name = library.Name ?? string.Empty,
                    CollectionType =
                        library.CollectionType ?? string.Empty,
                    Paths = library.Locations ?? []
                })
                .ToList();
        }
        catch (HttpRequestException)
        {
            return [];
        }
        catch (TaskCanceledException)
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<SeriesInfo>> GetSeriesAsync()
    {
        var libraryIds =
            await _librarySelectionService
                .GetSelectedLibraryIdsAsync();

        if (libraryIds.Count == 0)
        {
            return [];
        }

        return await _seriesClient.GetSeriesAsync(
            libraryIds);
    }

    public Task<SeriesDetail?> GetSeriesDetailAsync(string seriesId)
    {
        return _seriesClient.GetSeriesDetailAsync(seriesId);
    }

    public async Task<string?> GetItemWebUrlAsync(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        var settings = await GetSettingsAsync();

        if (settings is null ||
            string.IsNullOrWhiteSpace(settings.JellyfinServerUrl))
        {
            return null;
        }

        var baseUrl = settings.JellyfinServerUrl
            .Trim()
            .TrimEnd('/');

        return
            $"{baseUrl}/web/#/details?id=" +
            Uri.EscapeDataString(itemId);
    }

    private async Task<AppSettings?> GetSettingsAsync()
    {
        return await _dbContext.AppSettings
            .AsNoTracking()
            .SingleOrDefaultAsync(settings => settings.Id == 1);
    }

    private static bool HasValidJellyfinSettings(
        AppSettings? settings)
    {
        return settings is not null &&
               !string.IsNullOrWhiteSpace(
                   settings.JellyfinServerUrl) &&
               !string.IsNullOrWhiteSpace(
                   settings.JellyfinApiKey);
    }

    private static HttpRequestMessage CreateRequest(
        AppSettings settings,
        string endpoint)
    {
        var baseUrl = settings.JellyfinServerUrl!
            .Trim()
            .TrimEnd('/');

        var normalizedEndpoint = endpoint
            .Trim()
            .TrimStart('/');

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{baseUrl}/{normalizedEndpoint}");

        request.Headers.Add(
            "X-Emby-Token",
            settings.JellyfinApiKey);

        return request;
    }

    private sealed class JellyfinServerInfoResponse
    {
        public string? ServerName { get; set; }

        public string? Version { get; set; }

        public string? Id { get; set; }
    }

    private sealed class JellyfinLibraryResponse
    {
        public string? ItemId { get; set; }

        public string? Name { get; set; }

        public string? CollectionType { get; set; }

        public List<string>? Locations { get; set; }
    }
}