namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Manages receipt logo image assets in an application-managed store.
/// Persists stable relative paths (filenames) so the shared ReceiptSettings row
/// is portable across workstations that share the same database.
/// Sibling of <see cref="IReportImageAssetStore"/> — same strategy, separate store.
/// </summary>
public interface IReceiptImageAssetStore
{
    /// <summary>Copies the given image file to the receipt asset store and returns the stored relative path (filename).</summary>
    /// <param name="sourcePath">Absolute path to the picked image file.</param>
    /// <returns>The relative filename to persist in ReceiptSettings, or null if the copy failed.</returns>
    string? StoreLogo(string sourcePath);

    /// <summary>Resolves a stored relative path to an absolute file path. Returns null if the file doesn't exist.</summary>
    string? ResolveLogoPath(string? relativePath);

    /// <summary>Gets the absolute path to the receipt asset store directory.</summary>
    string GetReceiptAssetDirectory();
}
