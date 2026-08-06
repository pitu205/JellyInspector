namespace JellyInspector.Application.Models;

public class LibraryInfo
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string CollectionType { get; set; } = string.Empty;

    public List<string> Paths { get; set; } = [];
}