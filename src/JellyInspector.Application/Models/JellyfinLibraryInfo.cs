namespace JellyInspector.Application.Models;

public sealed class JellyfinLibraryInfo
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string CollectionType { get; init; } = string.Empty;

    public int SeriesCount { get; init; }

    public List<string> Locations { get; init; } = [];
}
