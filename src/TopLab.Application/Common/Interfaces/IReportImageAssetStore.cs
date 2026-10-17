namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Manages report header/footer image assets in an application-managed store.
/// Persists stable relative paths (filenames) so the shared ReportSettings row
/// is portable across workstations that share the same database.
/// </summary>
public interface IReportImageAssetStore
{
    /// <summary>Copies the given image file to the asset store and returns the stored relative path (filename).</summary>
    /// <param name="sourcePath">Absolute path to the picked image file.</param>
    /// <param name="target">Whether this is for the header or footer.</param>
    /// <returns>The relative filename to persist in ReportSettings, or null if the copy failed.</returns>
    string? StoreImage(string sourcePath, ReportImageTarget target);

    /// <summary>Resolves a stored relative path to an absolute file path. Returns null if the file doesn't exist.</summary>
    string? ResolveImagePath(string? relativePath);

    /// <summary>Gets the absolute path to the asset store directory.</summary>
    string GetAssetDirectory();
}

public enum ReportImageTarget
{
    Header,
    Footer
}
