namespace JellyInspector.Infrastructure.Jellyfin;

public interface IJellyfinApiClient
{
    Task<T?> GetAsync<T>(
        string endpoint,
        CancellationToken cancellationToken = default);

    Task<byte[]?> GetBytesAsync(
        string endpoint,
        CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string endpoint,
        CancellationToken cancellationToken = default);
}