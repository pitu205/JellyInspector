using JellyInspector.Application.Models;

namespace JellyInspector.Infrastructure.Jellyfin.Clients;

public interface IEpisodeClient
{
    Task<IReadOnlyList<EpisodeInfo>> GetEpisodesAsync(
        string seriesId,
        string seasonId,
        CancellationToken cancellationToken = default);
}