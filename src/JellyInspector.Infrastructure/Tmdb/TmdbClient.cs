using System.Net.Http.Json;
using System.Text.Json.Serialization;
using JellyInspector.Application.Models;
using JellyInspector.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JellyInspector.Infrastructure.Tmdb;

public sealed class TmdbClient : ITmdbClient
{
    private readonly HttpClient _http;
    private readonly JellyInspectorDbContext _db;

    public TmdbClient(
        HttpClient http,
        JellyInspectorDbContext db)
    {
        _http = http;
        _db = db;
    }

    public async Task<TmdbSeries?> GetSeriesAsync(
        string tmdbId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tmdbId))
        {
            return null;
        }

        var settings = await _db.AppSettings
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == 1,
                cancellationToken);

        if (settings is null ||
            string.IsNullOrWhiteSpace(settings.TmdbApiKey))
        {
            return null;
        }

        var apiKey = Uri.EscapeDataString(settings.TmdbApiKey);
        var escapedTmdbId = Uri.EscapeDataString(tmdbId);

        var seriesUrl =
            $"tv/{escapedTmdbId}" +
            $"?api_key={apiKey}" +
            "&language=es-ES";

        var seriesResponse =
            await _http.GetFromJsonAsync<TmdbSeriesResponse>(
                seriesUrl,
                cancellationToken);

        if (seriesResponse is null)
        {
            return null;
        }

        var seasons = new List<TmdbSeason>();

        foreach (var seasonSummary in seriesResponse.Seasons
                     .OrderBy(item => item.SeasonNumber))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var seasonUrl =
                $"tv/{escapedTmdbId}/season/{seasonSummary.SeasonNumber}" +
                $"?api_key={apiKey}" +
                "&language=es-ES";

            var seasonResponse =
                await _http.GetFromJsonAsync<TmdbSeasonResponse>(
                    seasonUrl,
                    cancellationToken);

            if (seasonResponse is null)
            {
                continue;
            }

            var episodes = seasonResponse.Episodes
                .Select(item => MapEpisode(
                    item,
                    seasonResponse.SeasonNumber))
                .OrderBy(item => item.EpisodeNumber)
                .ToList();

            seasons.Add(new TmdbSeason
            {
                SeasonNumber = seasonResponse.SeasonNumber,
                Name = seasonResponse.Name ?? string.Empty,
                AirDate = ToDateOnly(seasonResponse.AirDate)
                    ?? ToDateOnly(seasonSummary.AirDate),
                EpisodeCount = seasonSummary.EpisodeCount > 0
                    ? seasonSummary.EpisodeCount
                    : episodes.Count,
                Episodes = episodes
            });
        }

        return new TmdbSeries
        {
            Id = seriesResponse.Id,
            Name = seriesResponse.Name ?? string.Empty,
            OriginalName = seriesResponse.OriginalName ?? string.Empty,
            Overview = seriesResponse.Overview ?? string.Empty,
            Tagline = seriesResponse.Tagline ?? string.Empty,
            Status = seriesResponse.Status ?? string.Empty,
            InProduction = seriesResponse.InProduction,
            FirstAirDate = ToDateOnly(seriesResponse.FirstAirDate),
            LastAirDate = ToDateOnly(seriesResponse.LastAirDate),
            PosterPath = seriesResponse.PosterPath ?? string.Empty,
            BackdropPath = seriesResponse.BackdropPath ?? string.Empty,
            VoteAverage = seriesResponse.VoteAverage,
            VoteCount = seriesResponse.VoteCount,
            OriginalLanguage = seriesResponse.OriginalLanguage ?? string.Empty,
            NumberOfSeasons = seriesResponse.NumberOfSeasons,
            NumberOfEpisodes = seriesResponse.NumberOfEpisodes,
            Genres = seriesResponse.Genres
                .Select(item => item.Name ?? string.Empty)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList(),
            Networks = seriesResponse.Networks
                .Select(item => item.Name ?? string.Empty)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList(),
            ProductionCountries = seriesResponse.ProductionCountries
                .Select(item => item.Name ?? string.Empty)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList(),
            NextEpisodeToAir = seriesResponse.NextEpisodeToAir is null
                ? null
                : MapEpisode(
                    seriesResponse.NextEpisodeToAir,
                    seriesResponse.NextEpisodeToAir.SeasonNumber),
            LastEpisodeToAir = seriesResponse.LastEpisodeToAir is null
                ? null
                : MapEpisode(
                    seriesResponse.LastEpisodeToAir,
                    seriesResponse.LastEpisodeToAir.SeasonNumber),
            Seasons = seasons
        };
    }

    private static TmdbEpisode MapEpisode(
        TmdbEpisodeResponse item,
        int fallbackSeasonNumber)
    {
        return new TmdbEpisode
        {
            SeasonNumber = item.SeasonNumber > 0
                ? item.SeasonNumber
                : fallbackSeasonNumber,
            EpisodeNumber = item.EpisodeNumber,
            Name = item.Name ?? string.Empty,
            AirDate = ToDateOnly(item.AirDate),
            Runtime = item.Runtime ?? 0
        };
    }

    private static DateOnly? ToDateOnly(DateTime? value)
    {
        return value.HasValue
            ? DateOnly.FromDateTime(value.Value)
            : null;
    }

    private sealed class TmdbSeriesResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("original_name")]
        public string? OriginalName { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("tagline")]
        public string? Tagline { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("in_production")]
        public bool InProduction { get; set; }

        [JsonPropertyName("first_air_date")]
        public DateTime? FirstAirDate { get; set; }

        [JsonPropertyName("last_air_date")]
        public DateTime? LastAirDate { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("backdrop_path")]
        public string? BackdropPath { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

        [JsonPropertyName("vote_count")]
        public int VoteCount { get; set; }

        [JsonPropertyName("original_language")]
        public string? OriginalLanguage { get; set; }

        [JsonPropertyName("number_of_seasons")]
        public int NumberOfSeasons { get; set; }

        [JsonPropertyName("number_of_episodes")]
        public int NumberOfEpisodes { get; set; }

        [JsonPropertyName("genres")]
        public List<NamedResponse> Genres { get; set; } = [];

        [JsonPropertyName("networks")]
        public List<NamedResponse> Networks { get; set; } = [];

        [JsonPropertyName("production_countries")]
        public List<NamedResponse> ProductionCountries { get; set; } = [];

        [JsonPropertyName("next_episode_to_air")]
        public TmdbEpisodeResponse? NextEpisodeToAir { get; set; }

        [JsonPropertyName("last_episode_to_air")]
        public TmdbEpisodeResponse? LastEpisodeToAir { get; set; }

        [JsonPropertyName("seasons")]
        public List<TmdbSeasonSummaryResponse> Seasons { get; set; } = [];
    }

    private sealed class NamedResponse
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private sealed class TmdbSeasonSummaryResponse
    {
        [JsonPropertyName("season_number")]
        public int SeasonNumber { get; set; }

        [JsonPropertyName("air_date")]
        public DateTime? AirDate { get; set; }

        [JsonPropertyName("episode_count")]
        public int EpisodeCount { get; set; }
    }

    private sealed class TmdbSeasonResponse
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("season_number")]
        public int SeasonNumber { get; set; }

        [JsonPropertyName("air_date")]
        public DateTime? AirDate { get; set; }

        [JsonPropertyName("episodes")]
        public List<TmdbEpisodeResponse> Episodes { get; set; } = [];
    }

    private sealed class TmdbEpisodeResponse
    {
        [JsonPropertyName("season_number")]
        public int SeasonNumber { get; set; }

        [JsonPropertyName("episode_number")]
        public int EpisodeNumber { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("air_date")]
        public DateTime? AirDate { get; set; }

        [JsonPropertyName("runtime")]
        public int? Runtime { get; set; }
    }
}
