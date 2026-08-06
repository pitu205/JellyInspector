using System.Text.Json.Serialization;

namespace JellyInspector.Infrastructure.Tmdb.Models;

public sealed class TmdbSeries
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("number_of_seasons")]
    public int NumberOfSeasons { get; init; }
}