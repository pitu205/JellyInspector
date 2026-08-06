using Jellyfin.Data.Enums;
using Jellyfin.Plugin.JellyInspector.Api.Models;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.JellyInspector.Api.Controllers;

/// <summary>
/// API del Dashboard de JellyInspector.
/// </summary>
[ApiController]
[Authorize]
[Route("JellyInspector")]
[Produces("application/json")]
public sealed class DashboardController : ControllerBase
{
    private readonly ILibraryManager _libraryManager;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="DashboardController"/> class.
    /// </summary>
    /// <param name="libraryManager">
    /// Gestor interno de la biblioteca de Jellyfin.
    /// </param>
    public DashboardController(
        ILibraryManager libraryManager)
    {
        _libraryManager = libraryManager;
    }

    /// <summary>
    /// Obtiene el resumen real de la biblioteca.
    /// </summary>
    /// <returns>Resumen del Dashboard.</returns>
    [HttpGet("Dashboard")]
    [ProducesResponseType(
        typeof(DashboardSummary),
        StatusCodes.Status200OK)]
    public ActionResult<DashboardSummary> GetDashboard()
    {
        var series = _libraryManager.GetCount(
            CreateCountQuery(BaseItemKind.Series));

        var seasons = _libraryManager.GetCount(
            CreateCountQuery(BaseItemKind.Season));

        var episodes = _libraryManager.GetCount(
            CreateCountQuery(BaseItemKind.Episode));

        return Ok(new DashboardSummary
        {
            Series = series,
            Seasons = seasons,
            Episodes = episodes,
            GeneratedUtc = DateTime.UtcNow
        });
    }

    private static InternalItemsQuery CreateCountQuery(
        BaseItemKind itemType)
    {
        return new InternalItemsQuery
        {
            IncludeItemTypes = [itemType],
            Recursive = true,
            IsVirtualItem = false
        };
    }
}
