using JellyInspector.Domain.Entities;
using JellyInspector.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JellyInspector.Web.Endpoints;

public static class JellyfinImageEndpoint
{
    public static void MapJellyfinImageEndpoint(this WebApplication app)
    {
        app.MapGet("/api/jellyfin/image/{id}", async (
            string id,
            string? tag,
            JellyInspectorDbContext db,
            IHttpClientFactory httpFactory) =>
        {
            var settings = await db.AppSettings
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == 1);

            if (settings is null ||
                string.IsNullOrWhiteSpace(settings.JellyfinServerUrl) ||
                string.IsNullOrWhiteSpace(settings.JellyfinApiKey))
            {
                return Results.NotFound();
            }

            var client = httpFactory.CreateClient();

            var url =
                $"{settings.JellyfinServerUrl.TrimEnd('/')}/Items/{id}/Images/Primary";

            if (!string.IsNullOrWhiteSpace(tag))
                url += $"?tag={Uri.EscapeDataString(tag)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Add(
                "X-Emby-Token",
                settings.JellyfinApiKey);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return Results.NotFound();

            var stream = await response.Content.ReadAsStreamAsync();

            var contentType =
                response.Content.Headers.ContentType?.MediaType
                ?? "image/jpeg";

            return Results.File(stream, contentType);
        });
    }
}