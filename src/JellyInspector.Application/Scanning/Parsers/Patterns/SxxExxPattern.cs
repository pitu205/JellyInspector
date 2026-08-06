using System.Text.RegularExpressions;

namespace JellyInspector.Application.Scanning.Parsers.Patterns;

public sealed class SxxExxPattern : IEpisodePattern
{
    private static readonly Regex Regex = new(
        @"[Ss](?<season>\d{1,2})[\s._-]*[Ee](?<episode>\d{1,3})",
        RegexOptions.Compiled);

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