using JellyInspector.Application.Models;

namespace JellyInspector.Infrastructure.Jellyfin.Clients;

public interface ILibraryClient
{
    Task<IReadOnlyList<JellyfinLibraryInfo>> GetSeriesLibrariesAsync(
        CancellationToken cancellationToken = default);
}
