namespace JellyInspector.Infrastructure.Jellyfin.Clients;

public interface IUserClient
{
    Task<string?> GetCurrentUserIdAsync();
}