namespace JellyInspector.Application.Scanning.Parsers;

public interface IEpisodePattern
{
    bool TryParse(string fileName, out EpisodeInfo episode);
}