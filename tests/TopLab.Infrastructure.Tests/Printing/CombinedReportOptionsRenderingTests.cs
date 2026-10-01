using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>W-02 S8 (WP-13): the combined-report writer carries the off-lab note,
/// the test comments and the group sub-title switch.</summary>
public class CombinedReportOptionsRenderingTests
{
    private static string WriterSource()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(
                dir.FullName, "src", "TopLab.Infrastructure", "Printing", "ReportContentBuilder.cs");
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate ReportContentBuilder.cs");
    }

    [Fact]
    public void ReportContentBuilder_RendersOutsideLabNote()
    {
        Assert.Contains("العينة أُخذت خارج المعمل", WriterSource(), StringComparison.Ordinal);
    }

    [Fact]
    public void ReportContentBuilder_RendersTestCommentsAfterResult()
    {
        var source = WriterSource();

        Assert.Contains("TestComments", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ReportContentBuilder_HonoursGroupSubTitleSetting()
    {
        var source = WriterSource();

        Assert.Contains("PrintGroupSubTitle", source, StringComparison.Ordinal);
        Assert.Contains("المجموعة:", source, StringComparison.Ordinal);
    }
}
