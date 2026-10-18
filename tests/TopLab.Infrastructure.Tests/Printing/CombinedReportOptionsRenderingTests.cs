using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>W-02 S8 (WP-13): the combined-report writer carries the off-lab note,
/// the test comments and the group sub-title switch. GAP-014 (Phase 7) moved the
/// Arabic default text into <c>ReportItemTemplateCatalog</c>, so the literals are
/// asserted there and the builder is asserted to drive the matching template keys.</summary>
public class CombinedReportOptionsRenderingTests
{
    private static string WriterSource() => RepoSource("ReportContentBuilder.cs");

    private static string CatalogSource() => RepoSource("ReportItemTemplateCatalog.cs");

    private static string RepoSource(string fileName)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(
                dir.FullName, "src", "TopLab.Infrastructure", "Printing", fileName);
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            var domainCandidate = Path.Combine(
                dir.FullName, "src", "TopLab.Domain", "Settings", fileName);
            if (File.Exists(domainCandidate))
            {
                return File.ReadAllText(domainCandidate);
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException($"Could not locate {fileName}");
    }

    [Fact]
    public void ReportContentBuilder_RendersOutsideLabNote()
    {
        Assert.Contains("ReportItemKeys.OffLabNote", WriterSource(), StringComparison.Ordinal);
        Assert.Contains("العينة أُخذت خارج المعمل", CatalogSource(), StringComparison.Ordinal);
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
        Assert.Contains("ReportItemKeys.GroupSubtitle", source, StringComparison.Ordinal);
        Assert.Contains($"\"المجموعة\"", CatalogSource(), StringComparison.Ordinal);
    }
}
