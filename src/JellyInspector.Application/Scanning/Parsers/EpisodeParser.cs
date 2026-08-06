using JellyInspector.Application.Scanning.Parsers.Patterns;

namespace JellyInspector.Application.Scanning.Parsers;

public sealed class EpisodeParser
{
    private readonly List<IEpisodePattern> _patterns =
    [
        new SxxExxPattern(),
        new XPattern()
    ];

    public EpisodeInfo Parse(string fileName)
    {
        foreach (var parser in _patterns)
        {
            if (parser.TryParse(fileName, out var result))
                return result;
        }

        return new EpisodeInfo
        {
            Success = false,
            FileName = fileName
        };
    }
}