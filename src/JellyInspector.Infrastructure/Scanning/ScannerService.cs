using System.Diagnostics;
using JellyInspector.Application.Interfaces;
using JellyInspector.Application.Library;
using JellyInspector.Application.Models;
using JellyInspector.Application.Scanning;
using JellyInspector.Domain.Entities;
using JellyInspector.Infrastructure.Comparison;
using JellyInspector.Infrastructure.Jellyfin.Clients;
using JellyInspector.Infrastructure.Repositories;
using JellyInspector.Infrastructure.Tmdb;

namespace JellyInspector.Infrastructure.Scanning;

public sealed class ScannerService : IScannerService
{
    private readonly ISeriesClient _seriesClient;
    private readonly ILibrarySelectionService _librarySelectionService;
    private readonly ISeasonClient _seasonClient;
    private readonly IEpisodeClient _episodeClient;
    private readonly ISeriesRepository _seriesRepository;
    private readonly IScanIssueRepository _scanIssueRepository;
    private readonly IScanSessionRepository _scanSessionRepository;
    private readonly ITmdbClient _tmdbClient;
    private readonly LibraryComparisonService _comparisonService;

    private readonly object _scanSync = new();

    private Task<ScanResult>? _activeScanTask;
    private CancellationTokenSource? _activeScanCancellation;
    private Task<ScanResult>? _activeSeriesScanTask;
    private string? _activeSeriesScanId;

    public ScanProgress Progress { get; } = new();

    public bool IsRunning =>
        ActiveScanTask is { IsCompleted: false };

    public bool CanCancel
    {
        get
        {
            lock (_scanSync)
            {
                return _activeScanTask is { IsCompleted: false }
                       && _activeScanCancellation is not null
                       && !_activeScanCancellation.IsCancellationRequested;
            }
        }
    }

    public Task<ScanResult>? ActiveScanTask
    {
        get
        {
            lock (_scanSync)
            {
                return _activeScanTask;
            }
        }
    }

    public string? ActiveSeriesScanId
    {
        get
        {
            lock (_scanSync)
            {
                return _activeSeriesScanTask is { IsCompleted: false }
                    ? _activeSeriesScanId
                    : null;
            }
        }
    }

    public ScannerService(
        ISeriesClient seriesClient,
        ISeasonClient seasonClient,
        IEpisodeClient episodeClient,
        ISeriesRepository seriesRepository,
        IScanIssueRepository scanIssueRepository,
        IScanSessionRepository scanSessionRepository,
        ITmdbClient tmdbClient,
        LibraryComparisonService comparisonService,
        ILibrarySelectionService librarySelectionService)
    {
        _seriesClient = seriesClient;
        _seasonClient = seasonClient;
        _episodeClient = episodeClient;
        _seriesRepository = seriesRepository;
        _scanIssueRepository = scanIssueRepository;
        _scanSessionRepository = scanSessionRepository;
        _tmdbClient = tmdbClient;
        _comparisonService = comparisonService;
        _librarySelectionService = librarySelectionService;
    }

    public bool IsSeriesRunning(string jellyfinSeriesId)
    {
        if (string.IsNullOrWhiteSpace(jellyfinSeriesId))
        {
            return false;
        }

        lock (_scanSync)
        {
            return _activeSeriesScanTask is { IsCompleted: false }
                   && string.Equals(
                       _activeSeriesScanId,
                       jellyfinSeriesId,
                       StringComparison.OrdinalIgnoreCase);
        }
    }

    public Task<ScanResult> ScanAsync(
        string libraryPath,
        CancellationToken cancellationToken = default)
    {
        lock (_scanSync)
        {
            if (_activeScanTask is { IsCompleted: false })
            {
                return _activeScanTask;
            }

            if (_activeSeriesScanTask is { IsCompleted: false })
            {
                throw new InvalidOperationException(
                    "No se puede iniciar un escaneo completo mientras se analiza una serie.");
            }

            _activeScanCancellation?.Dispose();
            _activeScanCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken);

            _activeScanTask = RunFullScanManagedAsync(
                libraryPath,
                _activeScanCancellation.Token);

            return _activeScanTask;
        }
    }

    public Task CancelScanAsync()
    {
        lock (_scanSync)
        {
            if (_activeScanTask is { IsCompleted: false })
            {
                Progress.CurrentSeries =
                    "Cancelando escaneo...";

                _activeScanCancellation?.Cancel();
            }
        }

        return Task.CompletedTask;
    }

    private async Task<ScanResult> RunFullScanManagedAsync(
        string libraryPath,
        CancellationToken cancellationToken)
    {
        try
        {
            return await RunScanCoreAsync(
                libraryPath,
                cancellationToken);
        }
        finally
        {
            lock (_scanSync)
            {
                _activeScanCancellation?.Dispose();
                _activeScanCancellation = null;
            }
        }
    }

    public Task<ScanResult> ScanSeriesAsync(
        string jellyfinSeriesId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jellyfinSeriesId))
        {
            throw new ArgumentException(
                "El identificador de la serie es obligatorio.",
                nameof(jellyfinSeriesId));
        }

        lock (_scanSync)
        {
            if (_activeScanTask is { IsCompleted: false })
            {
                throw new InvalidOperationException(
                    "Hay un escaneo completo en ejecución.");
            }

            if (_activeSeriesScanTask is { IsCompleted: false })
            {
                if (string.Equals(
                    _activeSeriesScanId,
                    jellyfinSeriesId,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return _activeSeriesScanTask;
                }

                throw new InvalidOperationException(
                    "Ya se está analizando otra serie.");
            }

            _activeSeriesScanId = jellyfinSeriesId;
            _activeSeriesScanTask = RunSingleSeriesScanAsync(
                jellyfinSeriesId,
                cancellationToken);

            return _activeSeriesScanTask;
        }
    }

    private async Task<ScanResult> RunScanCoreAsync(
        string libraryPath,
        CancellationToken cancellationToken)
    {
        var startedUtc = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        ResetProgress();

        var selectedLibraryIds =
            await _librarySelectionService.GetSelectedLibraryIdsAsync(
                cancellationToken);

        if (selectedLibraryIds.Count == 0)
        {
            throw new InvalidOperationException(
                "No hay ninguna biblioteca de series seleccionada.");
        }

        var jellyfinSeries =
            await _seriesClient.GetSeriesAsync(
                selectedLibraryIds,
                cancellationToken);

        Progress.Total = jellyfinSeries.Count;

        var entities = new List<SeriesEntity>();
        var issues = new List<ScanIssue>();
        var totalSeasons = 0;
        var totalEpisodes = 0;

        foreach (var seriesInfo in jellyfinSeries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Progress.CurrentSeries = seriesInfo.Name;

            var scanned = await ScanSeriesInternalAsync(
                new SeriesScanSource
                {
                    JellyfinId = seriesInfo.Id,
                    Name = seriesInfo.Name,
                    ProductionYear = seriesInfo.ProductionYear,
                    Overview = seriesInfo.Overview,
                    ImageTag = seriesInfo.ImageTag,
                    TmdbId = seriesInfo.TmdbId,
                    TvdbId = seriesInfo.TvdbId,
                    ImdbId = seriesInfo.ImdbId
                },
                cancellationToken);

            entities.Add(scanned.Entity);
            issues.AddRange(scanned.Issues);
            totalSeasons += scanned.SeasonCount;
            totalEpisodes += scanned.EpisodeCount;

            Progress.Current++;
        }

        cancellationToken.ThrowIfCancellationRequested();

        Progress.CurrentSeries =
            "Guardando biblioteca en SQLite...";

        // Desde este punto se completa la escritura para no dejar SQLite
        // en un estado parcial aunque se pulse Detener.
        await _scanIssueRepository.ClearAsync(CancellationToken.None);
        await _seriesRepository.ClearAsync(CancellationToken.None);
        await _seriesRepository.SaveAsync(
            entities,
            CancellationToken.None);

        stopwatch.Stop();

        var finishedUtc = DateTime.UtcNow;
        var scanSession = CreateScanSession(
            startedUtc,
            finishedUtc,
            stopwatch.Elapsed,
            entities.Count,
            totalSeasons,
            totalEpisodes,
            issues.Count);

        Progress.CurrentSeries =
            "Guardando sesión de escaneo...";

        await _scanSessionRepository.SaveAsync(
            scanSession,
            CancellationToken.None);

        Progress.CurrentSeries =
            "Guardando incidencias...";

        await _scanIssueRepository.SaveAsync(
            CreateIssueEntities(
                issues,
                entities,
                scanSession.Id,
                finishedUtc),
            CancellationToken.None);

        Progress.CurrentSeries = "Escaneo completado";

        return CreateResult(
            entities.Count,
            totalSeasons,
            totalEpisodes,
            issues,
            stopwatch.Elapsed);
    }

    private async Task<ScanResult> RunSingleSeriesScanAsync(
        string jellyfinSeriesId,
        CancellationToken cancellationToken)
    {
        var startedUtc = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var existing =
                await _seriesRepository.GetByJellyfinIdAsync(
                    jellyfinSeriesId,
                    cancellationToken);

            var detail =
                await _seriesClient.GetSeriesDetailAsync(
                    jellyfinSeriesId,
                    cancellationToken);

            if (detail is null)
            {
                throw new InvalidOperationException(
                    "Jellyfin no ha devuelto la serie seleccionada.");
            }

            var scanned = await ScanSeriesInternalAsync(
                new SeriesScanSource
                {
                    JellyfinId = detail.Id,
                    Name = detail.Name,
                    ProductionYear = detail.ProductionYear,
                    Overview = detail.Overview,
                    ImageTag = detail.ImageTag,
                    TmdbId = detail.TmdbId,
                    TvdbId = existing?.TvdbId,
                    ImdbId = existing?.ImdbId
                },
                cancellationToken);

            await _seriesRepository.ReplaceAsync(
                scanned.Entity,
                cancellationToken);

            stopwatch.Stop();

            var lastFullSession =
                await _scanSessionRepository.GetLastAsync(
                    cancellationToken);

            if (lastFullSession is null)
            {
                throw new InvalidOperationException(
                    "Debe realizarse al menos un escaneo completo antes de escanear una serie individualmente.");
            }

            var finishedUtc = DateTime.UtcNow;

            await _scanIssueRepository.SaveAsync(
                CreateIssueEntities(
                    scanned.Issues,
                    [scanned.Entity],
                    lastFullSession.Id,
                    finishedUtc),
                cancellationToken);

            await _scanSessionRepository.RefreshIssueCountAsync(
                lastFullSession.Id,
                cancellationToken);

            return CreateResult(
                1,
                scanned.SeasonCount,
                scanned.EpisodeCount,
                scanned.Issues,
                stopwatch.Elapsed);
        }
        finally
        {
            lock (_scanSync)
            {
                _activeSeriesScanId = null;
            }
        }
    }

    private async Task<ScannedSeries> ScanSeriesInternalAsync(
        SeriesScanSource source,
        CancellationToken cancellationToken)
    {
        var entity = new SeriesEntity
        {
            Id = Guid.NewGuid(),
            JellyfinId = source.JellyfinId,
            Name = source.Name,
            ProductionYear = source.ProductionYear,
            Overview = source.Overview,
            ImageTag = source.ImageTag,
            TmdbId = source.TmdbId,
            TvdbId = source.TvdbId,
            ImdbId = source.ImdbId
        };

        var issues = new List<ScanIssue>();
        AddMetadataIssues(source, issues);

        var seasons = await _seasonClient.GetSeasonsAsync(
            source.JellyfinId,
            cancellationToken);

        var processedEpisodeIds = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        var episodeCount = 0;

        foreach (var seasonInfo in seasons)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var season = new SeasonEntity
            {
                Id = Guid.NewGuid(),
                JellyfinId = seasonInfo.Id,
                SeriesId = entity.Id,
                SeasonNumber = seasonInfo.IndexNumber,
                Name = seasonInfo.Name,
                Series = entity
            };

            var episodes = await _episodeClient.GetEpisodesAsync(
                source.JellyfinId,
                seasonInfo.Id,
                cancellationToken);

            foreach (var episodeInfo in episodes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(episodeInfo.Id)
                    || !processedEpisodeIds.Add(episodeInfo.Id))
                {
                    continue;
                }

                season.Episodes.Add(new EpisodeEntity
                {
                    Id = Guid.NewGuid(),
                    JellyfinId = episodeInfo.Id,
                    SeasonId = season.Id,
                    EpisodeNumber = episodeInfo.EpisodeNumber,
                    Name = episodeInfo.Name,
                    Runtime = episodeInfo.Runtime,
                    Resolution = episodeInfo.Resolution,
                    VideoCodec = episodeInfo.VideoCodec,
                    AudioCodec = episodeInfo.AudioCodec,
                    Bitrate = episodeInfo.Bitrate,
                    FileSize = episodeInfo.FileSize,
                    HasHdr = episodeInfo.HasHdr,
                    HasDolbyVision = episodeInfo.HasDolbyVision,
                    Season = season
                });

                episodeCount++;
            }

            entity.Seasons.Add(season);
        }

        if (!string.IsNullOrWhiteSpace(source.TmdbId))
        {
            try
            {
                var tmdbSeries = await _tmdbClient.GetSeriesAsync(
                    source.TmdbId,
                    cancellationToken);

                if (tmdbSeries is not null)
                {
                    entity.TmdbVoteAverage =
                        tmdbSeries.VoteAverage;

                    entity.TmdbVoteCount =
                        tmdbSeries.VoteCount;

                    issues.AddRange(
                        _comparisonService.Compare(entity, tmdbSeries));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"TMDb ERROR [{source.Name}] {ex.Message}");
            }
        }

        return new ScannedSeries
        {
            Entity = entity,
            Issues = issues,
            SeasonCount = entity.Seasons.Count,
            EpisodeCount = episodeCount
        };
    }

    private static void AddMetadataIssues(
        SeriesScanSource source,
        ICollection<ScanIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(source.ImageTag))
        {
            issues.Add(new ScanIssue
            {
                Type = ScanIssueType.MissingPoster,
                SeriesName = source.Name,
                Message = "La serie no tiene póster."
            });
        }

        if (string.IsNullOrWhiteSpace(source.Overview))
        {
            issues.Add(new ScanIssue
            {
                Type = ScanIssueType.MissingOverview,
                SeriesName = source.Name,
                Message = "La serie no tiene sinopsis."
            });
        }

        if (string.IsNullOrWhiteSpace(source.TmdbId))
        {
            issues.Add(new ScanIssue
            {
                Type = ScanIssueType.MissingTmdb,
                SeriesName = source.Name,
                Message = "La serie no tiene identificador TMDb."
            });
        }
    }

    private static ScanSessionEntity CreateScanSession(
        DateTime startedUtc,
        DateTime finishedUtc,
        TimeSpan duration,
        int series,
        int seasons,
        int episodes,
        int issueCount)
    {
        return new ScanSessionEntity
        {
            Id = Guid.NewGuid(),
            StartedUtc = startedUtc,
            FinishedUtc = finishedUtc,
            Duration = duration,
            Series = series,
            Seasons = seasons,
            Episodes = episodes,
            IssueCount = issueCount
        };
    }

    private static List<ScanIssueEntity> CreateIssueEntities(
        IEnumerable<ScanIssue> issues,
        IEnumerable<SeriesEntity> entities,
        Guid scanSessionId,
        DateTime createdUtc)
    {
        var seriesByName = entities
            .GroupBy(
                series => series.Name,
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.First(),
                StringComparer.OrdinalIgnoreCase);

        return issues
            .Where(issue => seriesByName.ContainsKey(issue.SeriesName))
            .Select(issue =>
            {
                var series = seriesByName[issue.SeriesName];

                return new ScanIssueEntity
                {
                    Id = Guid.NewGuid(),
                    ScanSessionId = scanSessionId,
                    SeriesId = series.Id,
                    Type = issue.Type.ToString(),
                    SeasonNumber = issue.SeasonNumber,
                    EpisodeNumber = issue.EpisodeNumber,
                    Message = issue.Message,
                    CreatedUtc = createdUtc
                };
            })
            .ToList();
    }

    private static ScanResult CreateResult(
        int series,
        int seasons,
        int episodes,
        List<ScanIssue> issues,
        TimeSpan duration)
    {
        return new ScanResult
        {
            Series = series,
            Seasons = seasons,
            Episodes = episodes,
            MissingEpisodes = issues.Count(issue =>
                issue.Type == ScanIssueType.MissingEpisode),
            Issues = issues,
            Duration = duration
        };
    }

    private void ResetProgress()
    {
        Progress.Current = 0;
        Progress.Total = 0;
        Progress.CurrentSeries = string.Empty;
    }

    private sealed class SeriesScanSource
    {
        public string JellyfinId { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public int ProductionYear { get; init; }
        public string? Overview { get; init; }
        public string? ImageTag { get; init; }
        public string? TmdbId { get; init; }
        public string? TvdbId { get; init; }
        public string? ImdbId { get; init; }
    }

    private sealed class ScannedSeries
    {
        public required SeriesEntity Entity { get; init; }
        public required List<ScanIssue> Issues { get; init; }
        public int SeasonCount { get; init; }
        public int EpisodeCount { get; init; }
    }
}
