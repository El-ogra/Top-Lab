using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// QuestPDF clinical-report document (WP-01). Arabic font + RTL, page size and
/// margins from <see cref="ReportSettings"/>, real header/footer blocks, doctor
/// signature line, and «صفحة X من Y» pagination. Follows the
/// <see cref="ReceiptPdfWriter"/> QuestPDF pattern.
/// </summary>
public sealed class ReportDocument : IDocument
{
    private readonly ReportDocumentContent _content;
    private readonly ReportSettings _settings;
    private readonly string _fontFamily;
    private readonly float _fontSize;

    public ReportDocument(
        ReportDocumentContent content,
        ReportSettings settings,
        string? fontFamily = null,
        float fontSize = 12f)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(settings);
        _content = content;
        _settings = settings;
#pragma warning disable CA1416 // Windows-only WPF app
        _fontFamily = ArabicFontResolver.Resolve(fontFamily);
#pragma warning restore CA1416
        _fontSize = fontSize > 0 ? fontSize : 12f;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            ReportPageComposer.ApplyPageGeometry(page, _settings);
            page.DefaultTextStyle(style => style
                .FontFamily(_fontFamily)
                .FontSize(_fontSize)
                .DirectionFromRightToLeft());
            page.ContentFromRightToLeft();

            page.Header().Element(c => ReportPageComposer.ComposeHeader(c, _content, _fontSize));
            page.Content().Element(c => ReportPageComposer.ComposeBody(c, _content, _fontSize, _fontFamily));
            page.Footer().Element(c => ReportPageComposer.ComposeFooter(c, _content, _fontSize));
        });
    }

    /// <summary>Golden-test entry point: all display strings in print order.</summary>
    public static IReadOnlyList<string> BuildDisplayLines(ReportDocumentContent content)
        => content.BuildDisplayLines();

    /// <summary>Writes this document to an absolute PDF path (fails if the file exists).</summary>
    public void WriteToFile(string absolutePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        var directory = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directory}");
        }

        if (File.Exists(absolutePath))
        {
            throw new IOException($"File already exists: {absolutePath}");
        }

        this.GeneratePdf(absolutePath);
    }
}
