using TopLab.Application.Common.Interfaces;
using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>
/// Phase 1 tests for the ReportImageAssetStore.
/// </summary>
public class ReportImageAssetStoreTests
{
    [Fact]
    public void StoreImage_ValidPng_ReturnsStoredFilename()
    {
        var store = new ReportImageAssetStore();
        var tempDir = Path.Combine(Path.GetTempPath(), "toplab-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var sourcePath = Path.Combine(tempDir, "source.png");
            File.WriteAllBytes(sourcePath, new byte[] { 1, 2, 3, 4 });

            var result = store.StoreImage(sourcePath, ReportImageTarget.Header);
            Assert.NotNull(result);
            Assert.Equal("Header.png", result);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void StoreImage_MissingFile_ReturnsNull()
    {
        var store = new ReportImageAssetStore();
        var result = store.StoreImage(@"C:\nonexistent\path\image.png", ReportImageTarget.Header);
        Assert.Null(result);
    }

    [Fact]
    public void StoreImage_UnsupportedExtension_ReturnsNull()
    {
        var store = new ReportImageAssetStore();
        var tempDir = Path.Combine(Path.GetTempPath(), "toplab-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var sourcePath = Path.Combine(tempDir, "source.txt");
            File.WriteAllText(sourcePath, "not an image");

            var result = store.StoreImage(sourcePath, ReportImageTarget.Header);
            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void ResolveImagePath_NullOrEmpty_ReturnsNull()
    {
        var store = new ReportImageAssetStore();
        Assert.Null(store.ResolveImagePath(null));
        Assert.Null(store.ResolveImagePath(""));
        Assert.Null(store.ResolveImagePath("   "));
    }

    [Fact]
    public void ResolveImagePath_PathTraversal_ReturnsNull()
    {
        var store = new ReportImageAssetStore();
        Assert.Null(store.ResolveImagePath(@"..\..\etc\passwd"));
        Assert.Null(store.ResolveImagePath("/etc/passwd"));
    }

    [Fact]
    public void ResolveImagePath_ExistingFile_ReturnsFullPath()
    {
        var store = new ReportImageAssetStore();
        var assetDir = store.GetAssetDirectory();
        Directory.CreateDirectory(assetDir);
        var testFile = Path.Combine(assetDir, "TestResolve.png");
        File.WriteAllBytes(testFile, new byte[] { 1, 2, 3 });
        try
        {
            var result = store.ResolveImagePath("TestResolve.png");
            Assert.Equal(testFile, result);
        }
        finally
        {
            File.Delete(testFile);
        }
    }

    [Fact]
    public void ResolveImagePath_NonExistingFile_ReturnsNull()
    {
        var store = new ReportImageAssetStore();
        var result = store.ResolveImagePath("NonExistentImage12345.png");
        Assert.Null(result);
    }
}
