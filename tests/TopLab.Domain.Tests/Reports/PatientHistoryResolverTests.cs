using TopLab.Domain.Common.Enums;
using TopLab.Domain.Reports;

namespace TopLab.Domain.Tests.Reports;

public class PatientHistoryResolverTests
{
    [Fact]
    public void ByLabCode_ReturnsTrimmedKey()
    {
        var key = PatientHistoryResolver.ResolveKey(HistorySortMode.ByLabCode, "  L-100  ", "Ahmed");

        Assert.Equal("L-100", key);
    }

    [Fact]
    public void ByLabCode_NullLabId_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            PatientHistoryResolver.ResolveKey(HistorySortMode.ByLabCode, null, "Ahmed"));

        Assert.Equal("labId", ex.ParamName);
    }

    [Fact]
    public void ByLabCode_EmptyLabId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            PatientHistoryResolver.ResolveKey(HistorySortMode.ByLabCode, "   ", "Ahmed"));
    }

    [Fact]
    public void ByPatientName_TrimsAndCaseFolds()
    {
        var key = PatientHistoryResolver.ResolveKey(HistorySortMode.ByPatientName, "L-1", "  Ahmed  Mohamed  ");

        Assert.Equal("AHMED MOHAMED", key);
    }

    [Fact]
    public void ByPatientName_InteriorWhitespace_CollapsesToSingleSpace()
    {
        var key = PatientHistoryResolver.ResolveKey(HistorySortMode.ByPatientName, null, "Ahmed    Mohamed");

        Assert.Equal("AHMED MOHAMED", key);
    }

    [Fact]
    public void ByPatientName_CaseAndWhitespaceVariants_NormalizeToSameKey()
    {
        var first = PatientHistoryResolver.ResolveKey(HistorySortMode.ByPatientName, null, "ahmed mohamed");
        var second = PatientHistoryResolver.ResolveKey(HistorySortMode.ByPatientName, null, "  AHMED MOHAMED  ");
        var third = PatientHistoryResolver.ResolveKey(HistorySortMode.ByPatientName, null, "Ahmed  Mohamed");

        Assert.Equal(first, second);
        Assert.Equal(first, third);
    }

    [Fact]
    public void ByPatientName_WhitespaceName_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            PatientHistoryResolver.ResolveKey(HistorySortMode.ByPatientName, "L-1", "   "));

        Assert.Equal("fullName", ex.ParamName);
    }

    [Fact]
    public void UnknownMode_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PatientHistoryResolver.ResolveKey((HistorySortMode)99, "L-1", "Ahmed"));
    }
}