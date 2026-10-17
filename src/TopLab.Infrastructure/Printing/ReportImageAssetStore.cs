using TopLab.Application.Common.Interfaces;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// File-system implementation of <see cref="IReportImageAssetStore"/>.
/// Stores images in <c>%ProgramData%\TopLab\ReportSettings\</c> with deterministic
/// filenames (Header.png / Footer.png) so the shared settings row is portable.
/// </summary>
public sealed class ReportImageAssetStore : IReportImageAssetStore
{
    private readonly string _assetDirectory;

    public ReportImageAssetStore()
    {
        var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        _assetDirectory = Path.Combine(programData, "TopLab", "ReportSettings");
    }

    public string GetAssetDirectory() => _assetDirectory;

    public string? StoreImage(string sourcePath, ReportImageTarget target)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

        if (!File.Exists(sourcePath))
        {
            return null;
        }

        var extension = Path.GetExtension(sourcePath).ToLowerInvariant();
        var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
        if (!allowedExtensions.Contains(extension))
        {
            return null;
        }

        // Validate file size (max 5 MB)
        var fileInfo = new FileInfo(sourcePath);
        if (fileInfo.Length > 5 * 1024 * 1024)
        {
            return null;
        }

        Directory.CreateDirectory(_assetDirectory);

        var targetFileName = target == ReportImageTarget.Header ? "Header" : "Footer";
        var destinationPath = Path.Combine(_assetDirectory, targetFileName + extension);

        // If the file is already in the asset store, just return the relative path
        if (string.Equals(Path.GetFullPath(sourcePath), Path.GetFullPath(destinationPath), StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetFileName(destinationPath);
        }

        File.Copy(sourcePath, destinationPath, overwrite: true);
        return Path.GetFileName(destinationPath);
    }

    public string? ResolveImagePath(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        // Security: only allow simple filenames, not path traversal
        var fileName = Path.GetFileName(relativePath);
        if (string.IsNullOrEmpty(fileName) || fileName != relativePath)
        {
            return null;
        }

        var fullPath = Path.Combine(_assetDirectory, fileName);
        return File.Exists(fullPath) ? fullPath : null;
    }
}
