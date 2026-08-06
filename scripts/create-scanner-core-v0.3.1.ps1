param(
    [string]$Root = "C:\JellyInspector\Application"
)

$ErrorActionPreference = "Stop"

$ScannerRoot = Join-Path $Root "src\JellyInspector.Scanner"
$Solution = Join-Path $Root "JellyInspector.sln"

if (-not (Test-Path $ScannerRoot)) {
    throw "No existe el proyecto JellyInspector.Scanner en: $ScannerRoot"
}

$Folders = @(
    "$ScannerRoot\Media",
    "$ScannerRoot\Results",
    "$ScannerRoot\Rules",
    "$ScannerRoot\Sources",
    "$ScannerRoot\Engine"
)

foreach ($Folder in $Folders) {
    New-Item -Path $Folder -ItemType Directory -Force | Out-Null
}

Remove-Item "$ScannerRoot\Class1.cs" -Force -ErrorAction SilentlyContinue

@'
namespace JellyInspector.Scanner.Media;

/// <summary>
/// Serie multimedia normalizada para el motor de análisis.
/// </summary>
public sealed class MediaSeries
{
    private readonly List<MediaSeason> _seasons = [];

    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int? ProductionYear { get; init; }

    public string? Overview { get; init; }

    public string? PosterPath { get; init; }

    public string? BackdropPath { get; init; }

    public string? TmdbId { get; init; }

    public string? TvdbId { get; init; }

    public string? ImdbId { get; init; }

    public IReadOnlyList<MediaSeason> Seasons => _seasons;

    public void AddSeason(MediaSeason season)
    {
        ArgumentNullException.ThrowIfNull(season);
        _seasons.Add(season);
    }

    public void AddSeasons(IEnumerable<MediaSeason> seasons)
    {
        ArgumentNullException.ThrowIfNull(seasons);

        foreach (var season in seasons)
        {
            AddSeason(season);
        }
    }
}
'@ | Set-Content "$ScannerRoot\Media\MediaSeries.cs" -Encoding UTF8

@'
namespace JellyInspector.Scanner.Media;

/// <summary>
/// Temporada multimedia normalizada.
/// </summary>
public sealed class MediaSeason
{
    private readonly List<MediaEpisode> _episodes = [];

    public string Id { get; init; } = string.Empty;

    public int Number { get; init; }

    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<MediaEpisode> Episodes => _episodes;

    public void AddEpisode(MediaEpisode episode)
    {
        ArgumentNullException.ThrowIfNull(episode);
        _episodes.Add(episode);
    }

    public void AddEpisodes(IEnumerable<MediaEpisode> episodes)
    {
        ArgumentNullException.ThrowIfNull(episodes);

        foreach (var episode in episodes)
        {
            AddEpisode(episode);
        }
    }
}
'@ | Set-Content "$ScannerRoot\Media\MediaSeason.cs" -Encoding UTF8

@'
namespace JellyInspector.Scanner.Media;

/// <summary>
/// Episodio multimedia normalizado.
/// </summary>
public sealed class MediaEpisode
{
    public string Id { get; init; } = string.Empty;

    public int SeasonNumber { get; init; }

    public int Number { get; init; }

    public string Title { get; init; } = string.Empty;

    public bool HasFile { get; init; }

    public string? FilePath { get; init; }

    public long? FileSize { get; init; }

    public string? Resolution { get; init; }

    public string? VideoCodec { get; init; }

    public string? AudioCodec { get; init; }

    public long? Bitrate { get; init; }

    public bool HasHdr { get; init; }

    public bool HasDolbyVision { get; init; }
}
'@ | Set-Content "$ScannerRoot\Media\MediaEpisode.cs" -Encoding UTF8

@'
namespace JellyInspector.Scanner.Results;

/// <summary>
/// Gravedad de una incidencia detectada.
/// </summary>
public enum ScanSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2,
    Critical = 3
}
'@ | Set-Content "$ScannerRoot\Results\ScanSeverity.cs" -Encoding UTF8

@'
namespace JellyInspector.Scanner.Results;

/// <summary>
/// Incidencia detectada por una regla.
/// </summary>
public sealed class ScanIssue
{
    public string RuleId { get; init; } = string.Empty;

    public string RuleName { get; init; } = string.Empty;

    public ScanSeverity Severity { get; init; }

    public string SeriesId { get; init; } = string.Empty;

    public string SeriesName { get; init; } = string.Empty;

    public int? SeasonNumber { get; init; }

    public int? EpisodeNumber { get; init; }

    public string Message { get; init; } = string.Empty;

    public string? RecommendedAction { get; init; }
}
'@ | Set-Content "$ScannerRoot\Results\ScanIssue.cs" -Encoding UTF8

@'
namespace JellyInspector.Scanner.Results;

/// <summary>
/// Estadísticas acumuladas de un escaneo.
/// </summary>
public sealed class ScanStatistics
{
    public int SeriesProcessed { get; internal set; }

    public int RulesExecuted { get; internal set; }

    public int IssuesDetected { get; internal set; }

    public int InfoIssues { get; internal set; }

    public int WarningIssues { get; internal set; }

    public int ErrorIssues { get; internal set; }

    public int CriticalIssues { get; internal set; }
}
'@ | Set-Content "$ScannerRoot\Results\ScanStatistics.cs" -Encoding UTF8

@'
namespace JellyInspector.Scanner.Results;

/// <summary>
/// Resultado completo de una ejecución del motor.
/// </summary>
public sealed class ScanResult
{
    private readonly List<ScanIssue> _issues = [];

    public DateTime StartedUtc { get; init; }

    public DateTime FinishedUtc { get; internal set; }

    public TimeSpan Duration =>
        FinishedUtc > StartedUtc
            ? FinishedUtc - StartedUtc
            : TimeSpan.Zero;

    public ScanStatistics Statistics { get; } = new();

    public IReadOnlyList<ScanIssue> Issues => _issues;

    internal void AddIssues(IEnumerable<ScanIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        foreach (var issue in issues)
        {
            _issues.Add(issue);
            Statistics.IssuesDetected++;

            switch (issue.Severity)
            {
                case ScanSeverity.Info:
                    Statistics.InfoIssues++;
                    break;

                case ScanSeverity.Warning:
                    Statistics.WarningIssues++;
                    break;

                case ScanSeverity.Error:
                    Statistics.ErrorIssues++;
                    break;

                case ScanSeverity.Critical:
                    Statistics.CriticalIssues++;
                    break;
            }
        }
    }
}
'@ | Set-Content "$ScannerRoot\Results\ScanResult.cs" -Encoding UTF8

@'
using JellyInspector.Scanner.Media;
using JellyInspector.Scanner.Results;

namespace JellyInspector.Scanner.Rules;

/// <summary>
/// Regla independiente ejecutada sobre una serie.
/// </summary>
public interface IScannerRule
{
    string Id { get; }

    string Name { get; }

    string Description { get; }

    string Category { get; }

    bool IsEnabledByDefault { get; }

    Task<IReadOnlyCollection<ScanIssue>> ExecuteAsync(
        MediaSeries series,
        CancellationToken cancellationToken = default);
}
'@ | Set-Content "$ScannerRoot\Rules\IScannerRule.cs" -Encoding UTF8

@'
using JellyInspector.Scanner.Media;

namespace JellyInspector.Scanner.Sources;

/// <summary>
/// Fuente de datos normalizada para el motor.
/// </summary>
public interface IScannerDataSource
{
    IAsyncEnumerable<MediaSeries> GetSeriesAsync(
        CancellationToken cancellationToken = default);
}
'@ | Set-Content "$ScannerRoot\Sources\IScannerDataSource.cs" -Encoding UTF8

@'
namespace JellyInspector.Scanner.Engine;

/// <summary>
/// Progreso actual del motor de análisis.
/// </summary>
public sealed class ScannerProgress
{
    public int ProcessedSeries { get; init; }

    public string CurrentSeries { get; init; } = string.Empty;

    public int RulesExecuted { get; init; }

    public int IssuesDetected { get; init; }
}
'@ | Set-Content "$ScannerRoot\Engine\ScannerProgress.cs" -Encoding UTF8

@'
using JellyInspector.Scanner.Results;
using JellyInspector.Scanner.Rules;
using JellyInspector.Scanner.Sources;

namespace JellyInspector.Scanner.Engine;

/// <summary>
/// Motor independiente de análisis de bibliotecas multimedia.
/// </summary>
public sealed class ScannerEngine
{
    private readonly IReadOnlyList<IScannerRule> _rules;

    public ScannerEngine(IEnumerable<IScannerRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        _rules = rules
            .Where(rule => rule.IsEnabledByDefault)
            .ToArray();
    }

    public async Task<ScanResult> ScanAsync(
        IScannerDataSource dataSource,
        IProgress<ScannerProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dataSource);

        var result = new ScanResult
        {
            StartedUtc = DateTime.UtcNow
        };

        await foreach (var series in dataSource
                           .GetSeriesAsync(cancellationToken)
                           .WithCancellation(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            result.Statistics.SeriesProcessed++;

            foreach (var rule in _rules)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var issues = await rule.ExecuteAsync(
                    series,
                    cancellationToken);

                result.Statistics.RulesExecuted++;
                result.AddIssues(issues);

                progress?.Report(new ScannerProgress
                {
                    ProcessedSeries =
                        result.Statistics.SeriesProcessed,

                    CurrentSeries =
                        series.Name,

                    RulesExecuted =
                        result.Statistics.RulesExecuted,

                    IssuesDetected =
                        result.Statistics.IssuesDetected
                });
            }
        }

        result.FinishedUtc = DateTime.UtcNow;

        return result;
    }
}
'@ | Set-Content "$ScannerRoot\Engine\ScannerEngine.cs" -Encoding UTF8

Write-Host ""
Write-Host "=== SCANNER CORE CREADO ==="
Write-Host ""

Get-ChildItem $ScannerRoot -Recurse -Filter *.cs |
    Select-Object FullName

Write-Host ""
Write-Host "=== COMPILANDO SOLUCION ==="
Write-Host ""

dotnet build $Solution

if ($LASTEXITCODE -ne 0) {
    throw "La compilación ha fallado."
}

Write-Host ""
Write-Host "Scanner Core v0.3.1 creado y compilado correctamente."
