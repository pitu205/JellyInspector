namespace JellyInspector.Application.Library;

public interface ILibrarySelectionService
{
    Task<IReadOnlyList<string>> GetSelectedLibraryIdsAsync(
        CancellationToken cancellationToken = default);

    Task SaveSelectedLibraryIdsAsync(
        IEnumerable<string> libraryIds,
        CancellationToken cancellationToken = default);
}
