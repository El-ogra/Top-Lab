using System.Reflection;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Domain.Tests.Tests;

public class WorkGroupLogItemTests
{
    [Fact]
    public void Create_SetsBothIds()
    {
        var logId = WorkGroupLogId.Create(1);
        var testId = TestId.Create(5);

        var item = WorkGroupLogItem.Create(logId, testId);

        Assert.Equal(logId, item.WorkGroupLogId);
        Assert.Equal(testId, item.TestId);
    }

    [Fact]
    public void Create_NullIds_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => WorkGroupLogItem.Create(null!, TestId.Create(5)));
        Assert.Throws<ArgumentNullException>(() => WorkGroupLogItem.Create(WorkGroupLogId.Create(1), null!));
    }

    [Fact]
    public void CtorShape_MatchesEfCoreMaterializationContract()
    {
        var type = typeof(WorkGroupLogItem);

        var publicParameterless = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
            .OrderBy(c => c.GetParameters().Length)
            .FirstOrDefault();

        Assert.NotNull(publicParameterless);
        Assert.Empty(publicParameterless!.GetParameters());

        var parameterized = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).ToList();
        Assert.Contains(parameterized, c => c.GetParameters().Length == 2);

        var publicParameterized = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
            .Where(c => c.GetParameters().Length > 0)
            .ToList();
        Assert.Empty(publicParameterized);
    }
}