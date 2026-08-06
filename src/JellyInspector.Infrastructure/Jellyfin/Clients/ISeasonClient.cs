using JellyInspector.Application.Models;

namespace JellyInspector.Infrastructure.Jellyfin.Clients;

public interface ISeasonClient
{
    Task<IReadOnlyList<SeasonInfo>> GetSeasonsAsync(
        string seriesId,
        CancellationToken cancellationToken = default);
}