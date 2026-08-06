namespace JellyInspector.Application.Library;

public interface ILibraryBrowserService
{
    Task<List<LibrarySeriesDto>> GetSeriesAsync(
        CancellationToken cancellationToken = default);
}