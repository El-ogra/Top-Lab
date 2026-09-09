using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Domain.Tests.Results;

public class ProfileResultItemTests
{
    private static ProfileResultItem CreateItem(
        string resultValue = "5.5",
        bool verified = false,
        bool printed = false)
    {
        return ProfileResultItem.Create(
            ProfileResultItemId.Create(1),
            PatientTestId.Create(1),
            AnalyteId.Create(1),
            resultValue,
            "mg/dL",
            null,
            verified,
            printed);
    }

    [Fact]
    public void Create_GuardsBlankValue()
    {
        Assert.Throws<ArgumentException>(() => ProfileResultItem.Create(
            ProfileResultItemId.Create(1),
            PatientTestId.Create(1),
            AnalyteId.Create(1),
            " "));
    }

    [Fact]
    public void Update_AllowsDraftEdit()
    {
        var item = CreateItem();

        item.Update("6.0", "mmol/L", ProfileResultFlag.High);

        Assert.Equal("6.0", item.ResultValue);
        Assert.Equal("mmol/L", item.Unit);
        Assert.Equal(ProfileResultFlag.High, item.Flag);
    }

    [Fact]
    public void Update_RejectsPrintedItem()
    {
        var item = CreateItem(printed: true);

        Assert.Throws<InvalidOperationException>(() => item.Update("6.0", null, null));
    }

    [Fact]
    public void Verify_Unverify_AreIdempotent_AndPrePrintOnly()
    {
        var item = CreateItem();

        item.Verify();
        item.Verify();
        Assert.True(item.IsVerified);

        item.Unverify();
        item.Unverify();
        Assert.False(item.IsVerified);
    }

    [Fact]
    public void MarkPrinted_RequiresVerified()
    {
        var item = CreateItem();

        Assert.Throws<InvalidOperationException>(() => item.MarkPrinted(7, DateTime.UtcNow));
    }

    [Fact]
    public void MarkPrinted_AllowsReprint_AndCounts()
    {
        var item = CreateItem(verified: true);

        item.MarkPrinted(7, DateTime.UtcNow);
        item.MarkPrinted(8, DateTime.UtcNow);

        Assert.True(item.IsPrinted);
        Assert.Equal(2, item.PrintCount);
        Assert.Equal(8, item.LastPrintedByUserId);
    }

    [Fact]
    public void Amend_UpdatesActiveValue_WithoutLifecycleChange()
    {
        var item = CreateItem(verified: true, printed: true);

        item.Amend("7.0", "ng/mL", ProfileResultFlag.Low);

        Assert.Equal("7.0", item.ResultValue);
        Assert.Equal("ng/mL", item.Unit);
        Assert.Equal(ProfileResultFlag.Low, item.Flag);
        Assert.True(item.IsPrinted);
        Assert.True(item.IsVerified);
    }

    [Fact]
    public void Amend_PostPrint_DoesNotThrow()
    {
        var item = CreateItem(printed: true);

        item.Amend("8.0", null, null);

        Assert.Equal("8.0", item.ResultValue);
    }
}