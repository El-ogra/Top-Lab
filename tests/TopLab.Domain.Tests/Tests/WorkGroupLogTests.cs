using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Domain.Tests.Tests;

public class WorkGroupLogTests
{
    [Fact]
    public void Create_Valid()
    {
        var log = WorkGroupLog.Create(WorkGroupLogId.Create(1), "Main Log");

        Assert.Equal("Main Log", log.Name);
        Assert.Empty(log.Items);
    }

    [Fact]
    public void Create_TrimsName()
    {
        var log = WorkGroupLog.Create(WorkGroupLogId.Create(1), "  Main Log  ");

        Assert.Equal("Main Log", log.Name);
    }

    [Fact]
    public void Create_EmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() => WorkGroupLog.Create(WorkGroupLogId.Create(1), " "));
    }

    [Fact]
    public void Rename_Valid()
    {
        var log = WorkGroupLog.Create(WorkGroupLogId.Create(1), "Main Log");

        log.Rename("Night Shift");

        Assert.Equal("Night Shift", log.Name);
    }

    [Fact]
    public void Rename_EmptyName_Throws()
    {
        var log = WorkGroupLog.Create(WorkGroupLogId.Create(1), "Main Log");

        Assert.Throws<ArgumentException>(() => log.Rename(""));
    }

    [Fact]
    public void AddItem_AddsTestToItems()
    {
        var log = WorkGroupLog.Create(WorkGroupLogId.Create(1), "Main Log");
        var testId = TestId.Create(5);

        log.AddItem(testId);

        Assert.True(log.ContainsTest(testId));
        Assert.Single(log.Items);
    }

    [Fact]
    public void AddItem_DuplicateTest_Throws()
    {
        var log = WorkGroupLog.Create(WorkGroupLogId.Create(1), "Main Log");
        var testId = TestId.Create(5);

        log.AddItem(testId);

        Assert.Throws<ArgumentException>(() => log.AddItem(testId));
    }

    [Fact]
    public void RemoveItem_RemovesTest()
    {
        var log = WorkGroupLog.Create(WorkGroupLogId.Create(1), "Main Log");
        var testId = TestId.Create(5);

        log.AddItem(testId);
        log.RemoveItem(testId);

        Assert.Empty(log.Items);
        Assert.False(log.ContainsTest(testId));
    }

    [Fact]
    public void RemoveItem_NonMember_Throws()
    {
        var log = WorkGroupLog.Create(WorkGroupLogId.Create(1), "Main Log");

        Assert.Throws<ArgumentException>(() => log.RemoveItem(TestId.Create(5)));
    }

    [Fact]
    public void ClearItems_EmptiesLog()
    {
        var log = WorkGroupLog.Create(WorkGroupLogId.Create(1), "Main Log");

        for (var i = 1; i <= 3; i++)
        {
            log.AddItem(TestId.Create(i));
        }

        log.ClearItems();

        Assert.Empty(log.Items);
    }
}