using TopLab.Domain.Common;
using Xunit;

namespace TopLab.Domain.Tests.Common;

public class BranchScopeTests
{
    [Fact]
    public void BranchScope_ExplicitRequest_Wins()
    {
        var scope = new BranchScope(Requested: 5, SystemDefault: 1, UserAssigned: 2);
        Assert.Equal(5, scope.Resolve());
    }

    [Fact]
    public void BranchScope_NullRequest_UsesUserAssigned()
    {
        var scope = new BranchScope(Requested: null, SystemDefault: 1, UserAssigned: 2);
        Assert.Equal(2, scope.Resolve());
    }

    [Fact]
    public void BranchScope_NullUserAssigned_UsesSystemDefault()
    {
        var scope = new BranchScope(Requested: null, SystemDefault: 1, UserAssigned: null);
        Assert.Equal(1, scope.Resolve());
    }

    [Fact]
    public void BranchScope_IsAllBranches_WhenNothingAssigned()
    {
        var scope = new BranchScope(Requested: null, SystemDefault: 1, UserAssigned: null);
        Assert.True(scope.IsAllBranches);
    }

    [Fact]
    public void SystemSettings_DefaultBranchNumberIsOne()
    {
        var s = TopLab.Domain.Settings.SystemSettings.CreateDefault();
        Assert.Equal(1, s.BranchNumber);
        s.SetBranchNumber(3);
        Assert.Equal(3, s.BranchNumber);
    }

    [Fact]
    public void User_BranchNull_InheritsSystemBranch()
    {
        var u = TopLab.Domain.Users.User.Create(
            TopLab.Domain.Common.Ids.UserId.Create(1), "u", "h", "w");
        Assert.Null(u.BranchNumber);
        u.SetBranchNumber(4);
        Assert.Equal(4, u.BranchNumber);
    }
}
