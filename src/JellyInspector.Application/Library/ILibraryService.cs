namespace JellyInspector.Application.Library;

public interface ILibraryService
{
    Task<IReadOnlyList<LibrarySeriesDto>> GetSeriesAsync(
        CancellationToken cancellationToken = default);
}