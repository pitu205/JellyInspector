namespace JellyInspector.Application.Scanning;

public interface ISeriesIssueService
{
    Task<IReadOnlyList<ScanIssue>> GetByJellyfinIdAsync(
        string jellyfinId,
        CancellationToken cancellationToken = default);
}