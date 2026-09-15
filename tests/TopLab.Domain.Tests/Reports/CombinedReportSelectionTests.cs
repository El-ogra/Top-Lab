using TopLab.Domain.Reports;

namespace TopLab.Domain.Tests.Reports;

public class CombinedReportSelectionTests
{
    [Fact]
    public void NewSelection_IsEmpty()
    {
        var selection = new CombinedReportSelection();

        Assert.Equal(0, selection.Count);
        Assert.Empty(selection.OrderedIds);
    }

    [Fact]
    public void Add_Reviewed_PreservesOrder()
    {
        var selection = new CombinedReportSelection();

        selection.Add(1, isReviewed: true);
        selection.Add(2, isReviewed: true);
        selection.Add(3, isReviewed: true);

        Assert.Equal(new[] { 1, 2, 3 }, selection.OrderedIds);
        Assert.Equal(3, selection.Count);
    }

    [Fact]
    public void Add_NonReviewed_Throws()
    {
        var selection = new CombinedReportSelection();

        var ex = Assert.Throws<ArgumentException>(() => selection.Add(1, isReviewed: false));
        Assert.Equal("isReviewed", ex.ParamName);
        Assert.Equal(0, selection.Count);
    }

    [Fact]
    public void Add_Duplicate_Throws()
    {
        var selection = new CombinedReportSelection();
        selection.Add(7, isReviewed: true);

        var ex = Assert.Throws<ArgumentException>(() => selection.Add(7, isReviewed: true));
        Assert.Equal("patientTestId", ex.ParamName);
        Assert.Equal(1, selection.Count);
    }

    [Fact]
    public void MoveUp_KeepsContiguousSequence()
    {
        var selection = new CombinedReportSelection();
        selection.Add(1, isReviewed: true);
        selection.Add(2, isReviewed: true);
        selection.Add(3, isReviewed: true);

        selection.MoveUp(3);

        Assert.Equal(new[] { 1, 3, 2 }, selection.OrderedIds);
        Assert.Equal(3, selection.Count);
    }

    [Fact]
    public void MoveUp_AlreadyAtTop_NoOp()
    {
        var selection = new CombinedReportSelection();
        selection.Add(1, isReviewed: true);
        selection.Add(2, isReviewed: true);

        selection.MoveUp(1);

        Assert.Equal(new[] { 1, 2 }, selection.OrderedIds);
    }

    [Fact]
    public void MoveDown_KeepsContiguousSequence()
    {
        var selection = new CombinedReportSelection();
        selection.Add(1, isReviewed: true);
        selection.Add(2, isReviewed: true);
        selection.Add(3, isReviewed: true);

        selection.MoveDown(1);

        Assert.Equal(new[] { 2, 1, 3 }, selection.OrderedIds);
        Assert.Equal(3, selection.Count);
    }

    [Fact]
    public void MoveDown_AlreadyAtBottom_NoOp()
    {
        var selection = new CombinedReportSelection();
        selection.Add(1, isReviewed: true);
        selection.Add(2, isReviewed: true);

        selection.MoveDown(2);

        Assert.Equal(new[] { 1, 2 }, selection.OrderedIds);
    }

    [Fact]
    public void Move_UnknownId_Throws()
    {
        var selection = new CombinedReportSelection();
        selection.Add(1, isReviewed: true);

        Assert.Throws<ArgumentException>(() => selection.MoveUp(99));
        Assert.Throws<ArgumentException>(() => selection.MoveDown(99));
    }

    [Fact]
    public void RepeatedMoves_KeepSequenceContiguous()
    {
        var selection = new CombinedReportSelection();
        selection.Add(1, isReviewed: true);
        selection.Add(2, isReviewed: true);
        selection.Add(3, isReviewed: true);
        selection.Add(4, isReviewed: true);

        selection.MoveUp(4);
        selection.MoveUp(4);
        selection.MoveDown(4);

        Assert.Equal(new[] { 1, 2, 4, 3 }, selection.OrderedIds);
    }
}