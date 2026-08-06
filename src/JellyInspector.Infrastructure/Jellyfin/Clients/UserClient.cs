using System.Text.Json;

namespace JellyInspector.Infrastructure.Jellyfin.Clients;

public class UserClient : IUserClient
{
    private readonly IJellyfinApiClient _api;

    public UserClient(IJellyfinApiClient api)
    {
        _api = api;
    }

    public async Task<string?> GetCurrentUserIdAsync()
    {
        var users = await _api.GetAsync<JsonElement>("Users");

        if (users.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var user in users.EnumerateArray())
        {
            if (user.TryGetProperty("Policy", out var policy) &&
                policy.TryGetProperty("IsAdministrator", out var admin) &&
                admin.GetBoolean())
            {
                return user.GetProperty("Id").GetString();
            }
        }

        foreach (var user in users.EnumerateArray())
        {
            return user.GetProperty("Id").GetString();
        }

        return null;
    }
}