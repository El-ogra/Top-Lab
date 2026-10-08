using TopLab.Application.Features.PatientEnvelope.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

public class EnvelopePdfWriterTests
{
    private static EnvelopeDto Dto(string? referral = "د. أحمد")
    {
        return new EnvelopeDto(
            7,
            "أحمد محمد علي",
            "100",
            referral,
            new DateTime(2026, 10, 1, 9, 30, 0, DateTimeKind.Utc),
            "100");
    }

    private static EnvelopeSettings Settings(HeaderFooterMode mode = HeaderFooterMode.Words, bool suppressCaptions = false)
    {
        var settings = EnvelopeSettings.CreateDefault();
        settings.Update(3.0m, mode, suppressCaptions);
        return settings;
    }

    private static List<EnvelopePrintItemPosition> Positions(bool codeEnabled = true)
    {
        return new List<EnvelopePrintItemPosition>
        {
            new("Name", true, 1.0m, 1.0m),
            new("Code", codeEnabled, 1.0m, 2.0m),
            new("ReferralEntity", true, 1.0m, 3.0m),
            new("Date", true, 1.0m, 4.0m)
        };
    }

    private static LabPrintTextDto LabText()
    {
        return new LabPrintTextDto("مختبر الشفاء", "شارع الجمهورية", "01000000000", "Arial", 12);
    }

    [Fact]
    public void BuildTextLines_AllEnabled_MapsFourItemsInCanonicalOrder()
    {
        var lines = EnvelopePdfWriter.BuildTextLines(Dto(), Settings(), Positions(), LabText());

        Assert.Equal(["Name", "Code", "ReferralEntity", "Date"], lines.Items.Select(i => i.ItemName).ToList());
        Assert.Contains("أحمد محمد علي", lines.Items[0].Text);
        Assert.Contains("100", lines.Items[1].Text);
        Assert.Contains("د. أحمد", lines.Items[2].Text);
        Assert.Contains("2026-10-01 09:30", lines.Items[3].Text);
        Assert.Equal("100", lines.CodeBarcodePayload);
    }

    [Fact]
    public void BuildTextLines_CaptionsOn_PrefixesArabicCaptions()
    {
        var lines = EnvelopePdfWriter.BuildTextLines(Dto(), Settings(suppressCaptions: false), Positions(), LabText());

        Assert.StartsWith("المريض:", lines.Items[0].Text);
        Assert.StartsWith("الكود:", lines.Items[1].Text);
        Assert.StartsWith("الجهة المحولة:", lines.Items[2].Text);
        Assert.StartsWith("التاريخ:", lines.Items[3].Text);
    }

    [Fact]
    public void BuildTextLines_SuppressCaptions_RendersBareValues()
    {
        var lines = EnvelopePdfWriter.BuildTextLines(Dto(), Settings(suppressCaptions: true), Positions(), LabText());

        Assert.Equal("أحمد محمد علي", lines.Items[0].Text);
        Assert.Equal("100", lines.Items[1].Text);
        Assert.DoesNotContain("المريض", lines.Items[0].Text);
    }

    [Fact]
    public void BuildTextLines_HeaderWords_IncludesLabWords()
    {
        var lines = EnvelopePdfWriter.BuildTextLines(Dto(), Settings(HeaderFooterMode.Words), Positions(), LabText());

        Assert.Contains("مختبر الشفاء", lines.Header);
    }

    [Fact]
    public void BuildTextLines_HeaderNone_OmitsHeader()
    {
        var lines = EnvelopePdfWriter.BuildTextLines(Dto(), Settings(HeaderFooterMode.None), Positions(), LabText());

        Assert.Empty(lines.Header);
    }

    [Fact]
    public void BuildTextLines_HeaderImages_FallsBackToWords()
    {
        var lines = EnvelopePdfWriter.BuildTextLines(Dto(), Settings(HeaderFooterMode.Images), Positions(), LabText());

        Assert.Contains("مختبر الشفاء", lines.Header);
    }

    [Fact]
    public void BuildTextLines_CodeDisabled_OmitsBarcodeCleanly()
    {
        var lines = EnvelopePdfWriter.BuildTextLines(Dto(), Settings(), Positions(codeEnabled: false), LabText());

        Assert.DoesNotContain(lines.Items, i => i.ItemName == "Code");
        Assert.Null(lines.CodeBarcodePayload);
        Assert.Equal(3, lines.Items.Count);
    }

    [Fact]
    public void BuildTextLines_MissingReferral_OmitsReferralItem()
    {
        var lines = EnvelopePdfWriter.BuildTextLines(Dto(referral: null), Settings(), Positions(), LabText());

        Assert.DoesNotContain(lines.Items, i => i.ItemName == "ReferralEntity");
        Assert.Equal(3, lines.Items.Count);
        Assert.Equal("100", lines.CodeBarcodePayload);
    }

    [Fact]
    public void BuildTextLines_UnknownPositionName_IsIgnored()
    {
        var positions = Positions();
        positions.Add(new EnvelopePrintItemPosition("Extra", true, 1.0m, 5.0m));

        var lines = EnvelopePdfWriter.BuildTextLines(Dto(), Settings(), positions, LabText());

        Assert.Equal(4, lines.Items.Count);
    }

    [Fact]
    public void BuildTextLines_NullEnvelope_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            EnvelopePdfWriter.BuildTextLines(null!, Settings(), Positions(), LabText()));
    }
}
