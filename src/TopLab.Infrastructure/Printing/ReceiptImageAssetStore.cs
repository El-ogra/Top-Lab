using TopLab.Application.Common.Interfaces;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// File-system implementation of <see cref="IReceiptImageAssetStore"/>.
/// Stores the logo in <c>%ProgramData%\TopLab\ReceiptSettings\</c> with a deterministic
/// filename (Logo.ext) so the shared settings row is portable.
/// Sibling of <see cref="ReportImageAssetStore"/> — same strategy, separate directory.
/// </summary>
public sealed class ReceiptImageAssetStore : IReceiptImageAssetStore
{
    private readonly string _assetDirectory;

    public ReceiptImageAssetStore()
    {
        var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        _assetDirectory = Path.Combine(programData, "TopLab", "ReceiptSettings");
    }

    public string GetReceiptAssetDirectory() => _assetDirectory;

    public string? StoreLogo(string sourcePath)
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

        // Validate file size (max 5 MB — same limit as the report asset store)
        var fileInfo = new FileInfo(sourcePath);
        if (fileInfo.Length > 5 * 1024 * 1024)
        {
            return null;
        }

        Directory.CreateDirectory(_assetDirectory);

        var destinationPath = Path.Combine(_assetDirectory, "Logo" + extension);

        // If the file is already in the asset store, just return the relative path
        if (string.Equals(Path.GetFullPath(sourcePath), Path.GetFullPath(destinationPath), StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetFileName(destinationPath);
        }

        File.Copy(sourcePath, destinationPath, overwrite: true);
        return Path.GetFileName(destinationPath);
    }

    public string? ResolveLogoPath(string? relativePath)
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
