using System.Text.RegularExpressions;

namespace JellyInspector.Application.Scanning.Parsers.Patterns;

public sealed class XPattern : IEpisodePattern
{
    private static readonly Regex Regex = new(
        @"(?<season>\d{1,2})x(?<episode>\d{1,3})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public bool TryParse(string fileName, out EpisodeInfo episode)
    {
        episode = new EpisodeInfo();

        var match = Regex.Match(fileName);

        if (!match.Success)
            return false;

        episode = new EpisodeInfo
        {
            Success = true,
            Season = int.Parse(match.Groups["season"].Value),
            Episode = int.Parse(match.Groups["episode"].Value),
            FileName = fileName
        };

        return true;
    }
}