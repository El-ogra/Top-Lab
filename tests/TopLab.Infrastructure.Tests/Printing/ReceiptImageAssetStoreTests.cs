using TopLab.Application.Common.Interfaces;
using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>
/// GAP-007 tests for the ReceiptImageAssetStore.
/// </summary>
public class ReceiptImageAssetStoreTests
{
    [Fact]
    public void StoreLogo_ValidPng_ReturnsStoredFilename()
    {
        var store = new ReceiptImageAssetStore();
        var tempDir = Path.Combine(Path.GetTempPath(), "toplab-receipt-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var sourcePath = Path.Combine(tempDir, "source.png");
            File.WriteAllBytes(sourcePath, new byte[] { 1, 2, 3, 4 });

            var result = store.StoreLogo(sourcePath);
            Assert.NotNull(result);
            Assert.Equal("Logo.png", result);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void StoreLogo_MissingFile_ReturnsNull()
    {
        var store = new ReceiptImageAssetStore();
        var result = store.StoreLogo(@"C:\nonexistent\path\logo.png");
        Assert.Null(result);
    }

    [Fact]
    public void StoreLogo_UnsupportedExtension_ReturnsNull()
    {
        var store = new ReceiptImageAssetStore();
        var tempDir = Path.Combine(Path.GetTempPath(), "toplab-receipt-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var sourcePath = Path.Combine(tempDir, "source.txt");
            File.WriteAllText(sourcePath, "not an image");

            var result = store.StoreLogo(sourcePath);
            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void StoreLogo_OversizedFile_ReturnsNull()
    {
        var store = new ReceiptImageAssetStore();
        var tempDir = Path.Combine(Path.GetTempPath(), "toplab-receipt-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var sourcePath = Path.Combine(tempDir, "large.png");
            // Create a file larger than 5 MB
            using (var fs = new FileStream(sourcePath, FileMode.Create))
            {
                fs.SetLength(6 * 1024 * 1024);
            }

            var result = store.StoreLogo(sourcePath);
            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void ResolveLogoPath_NullOrEmpty_ReturnsNull()
    {
        var store = new ReceiptImageAssetStore();
        Assert.Null(store.ResolveLogoPath(null));
        Assert.Null(store.ResolveLogoPath(""));
        Assert.Null(store.ResolveLogoPath("   "));
    }

    [Fact]
    public void ResolveLogoPath_PathTraversal_ReturnsNull()
    {
        var store = new ReceiptImageAssetStore();
        Assert.Null(store.ResolveLogoPath(@"..\..\etc\passwd"));
        Assert.Null(store.ResolveLogoPath("/etc/passwd"));
    }

    [Fact]
    public void ResolveLogoPath_ExistingFile_ReturnsFullPath()
    {
        var store = new ReceiptImageAssetStore();
        var assetDir = store.GetReceiptAssetDirectory();
        Directory.CreateDirectory(assetDir);
        var testFile = Path.Combine(assetDir, "TestLogo.png");
        File.WriteAllBytes(testFile, new byte[] { 1, 2, 3 });
        try
        {
            var result = store.ResolveLogoPath("TestLogo.png");
            Assert.Equal(testFile, result);
        }
        finally
        {
            File.Delete(testFile);
        }
    }

    [Fact]
    public void ResolveLogoPath_NonExistingFile_ReturnsNull()
    {
        var store = new ReceiptImageAssetStore();
        var result = store.ResolveLogoPath("NonExistentLogo12345.png");
        Assert.Null(result);
    }

    [Fact]
    public void GetReceiptAssetDirectory_EndsWithReceiptSettings()
    {
        var store = new ReceiptImageAssetStore();
        var dir = store.GetReceiptAssetDirectory();
        Assert.EndsWith("ReceiptSettings", dir);
    }

    [Fact]
    public void StoreLogo_JpgExtension_ReturnsLogoJpg()
    {
        var store = new ReceiptImageAssetStore();
        var tempDir = Path.Combine(Path.GetTempPath(), "toplab-receipt-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var sourcePath = Path.Combine(tempDir, "photo.jpg");
            File.WriteAllBytes(sourcePath, new byte[] { 1, 2, 3, 4 });

            var result = store.StoreLogo(sourcePath);
            Assert.NotNull(result);
            Assert.Equal("Logo.jpg", result);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }
}
