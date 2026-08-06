namespace JellyInspector.Application.Scanning;

public enum ScanIssueType
{
    MissingEpisode,
    MissingSeason,
    DuplicateEpisode,
    MissingPoster,
    MissingOverview,
    MissingTmdb,
    MetadataMismatch,
    Airing,
    Upcoming
}