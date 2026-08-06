using JellyInspector.Application.Library;
using JellyInspector.Domain.Entities;
using JellyInspector.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JellyInspector.Infrastructure.Library;

public sealed class LibrarySelectionService
    : ILibrarySelectionService
{
    private readonly JellyInspectorDbContext _db;

    public LibrarySelectionService(
        JellyInspectorDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<string>> GetSelectedLibraryIdsAsync(
        CancellationToken cancellationToken = default)
    {
        _db.ChangeTracker.Clear();

        var value = await _db.AppSettings
            .AsNoTracking()
            .Where(item => item.Id == 1)
            .Select(item => item.SelectedSeriesLibraryIds)
            .SingleOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Split(
                ';',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Where(item =>
                !string.IsNullOrWhiteSpace(item))
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task SaveSelectedLibraryIdsAsync(
        IEnumerable<string> libraryIds,
        CancellationToken cancellationToken = default)
    {
        var normalized = libraryIds
            .Where(item =>
                !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .OrderBy(
                item => item,
                StringComparer.OrdinalIgnoreCase)
            .ToList();

        var serialized =
            string.Join(';', normalized);

        await SaveInternalAsync(
            serialized,
            cancellationToken);
    }

    private async Task SaveInternalAsync(
        string serializedLibraryIds,
        CancellationToken cancellationToken)
    {
        _db.ChangeTracker.Clear();

        var settings = await _db.AppSettings
            .SingleOrDefaultAsync(
                item => item.Id == 1,
                cancellationToken);

        if (settings is null)
        {
            settings = new AppSettings
            {
                Id = 1,
                SelectedSeriesLibraryIds =
                    serializedLibraryIds
            };

            _db.AppSettings.Add(settings);
        }
        else
        {
            settings.SelectedSeriesLibraryIds =
                serializedLibraryIds;
        }

        try
        {
            await _db.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // La fila fue modificada por otro DbContext durante
            // la sesión interactiva. Se descarta el estado local
            // y se vuelve a aplicar la selección sobre la fila actual.
            _db.ChangeTracker.Clear();

            var currentSettings =
                await _db.AppSettings
                    .SingleOrDefaultAsync(
                        item => item.Id == 1,
                        cancellationToken);

            if (currentSettings is null)
            {
                currentSettings = new AppSettings
                {
                    Id = 1,
                    SelectedSeriesLibraryIds =
                        serializedLibraryIds
                };

                _db.AppSettings.Add(
                    currentSettings);
            }
            else
            {
                currentSettings.SelectedSeriesLibraryIds =
                    serializedLibraryIds;
            }

            await _db.SaveChangesAsync(
                cancellationToken);
        }
    }
}
