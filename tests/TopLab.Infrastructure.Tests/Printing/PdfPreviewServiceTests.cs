using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

public class PdfPreviewServiceTests
{
    [Fact]
    public async Task PdfPreviewService_CreatesFileBeforeReturning()
    {
        var source = Path.Combine(Path.GetTempPath(), $"toplab-src-{Guid.NewGuid():N}.pdf");
        await File.WriteAllTextAsync(source, "%PDF-1.4 test");
        string? opened = null;
        var service = new PdfPreviewService(p => opened = p);

        try
        {
            var result = await service.PreviewAsync(source);

            Assert.True(result.IsSuccess, result.Error?.Message);
            Assert.NotNull(result.Value);
            Assert.True(File.Exists(result.Value!), "Preview file must exist before return.");
            Assert.Equal(opened, result.Value);
            Assert.Contains("TopLab-PDF-Preview", result.Value!, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(source);
            if (File.Exists(opened))
            {
                File.Delete(opened!);
            }
        }
    }

    [Fact]
    public async Task PdfPreviewService_ReturnsErrorOnFailure()
    {
        var missing = Path.Combine(Path.GetTempPath(), $"toplab-missing-{Guid.NewGuid():N}.pdf");
        var service = new PdfPreviewService(_ => { });

        var result = await service.PreviewAsync(missing);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("المعاينة", result.Error!.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PdfPreviewService_ReturnsError_WhenOpenThrows()
    {
        var source = Path.Combine(Path.GetTempPath(), $"toplab-src-{Guid.NewGuid():N}.pdf");
        await File.WriteAllTextAsync(source, "%PDF-1.4 test");
        var service = new PdfPreviewService(_ => throw new InvalidOperationException("no viewer"));

        try
        {
            var result = await service.PreviewAsync(source);
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Error);
        }
        finally
        {
            File.Delete(source);
        }
    }
}
