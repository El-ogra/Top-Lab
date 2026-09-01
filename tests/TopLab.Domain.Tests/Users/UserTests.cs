using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;

namespace TopLab.Domain.Tests.Users;

public class UserTests
{
    private static User CreateUser(
        string userName = "testuser",
        bool isAbsolute = false,
        decimal discount = 0,
        bool blockPrint = false)
    {
        return User.Create(
            UserId.Create(1),
            userName,
            "hash_main",
            "hash_secondary",
            isAbsolute,
            discount,
            blockPrint);
    }

    [Fact]
    public void ChangeUserName_Valid_TrimsAndSets()
    {
        var user = CreateUser("old");
        user.ChangeUserName("  newName  ");
        Assert.Equal("newName", user.UserName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ChangeUserName_Invalid_Throws(string? input)
    {
        var user = CreateUser();
        Assert.Throws<ArgumentException>(() => user.ChangeUserName(input!));
    }

    [Fact]
    public void ChangePasswordHash_Valid_Sets()
    {
        var user = CreateUser();
        user.ChangePasswordHash("new_hash_value");
        Assert.Equal("new_hash_value", user.PasswordHash);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ChangePasswordHash_Invalid_Throws(string? input)
    {
        var user = CreateUser();
        Assert.Throws<ArgumentException>(() => user.ChangePasswordHash(input!));
    }

    [Fact]
    public void ChangeInternalWindowsPasswordHash_Valid_Sets()
    {
        var user = CreateUser();
        user.ChangeInternalWindowsPasswordHash("new_secondary_hash");
        Assert.Equal("new_secondary_hash", user.InternalWindowsPasswordHash);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ChangeInternalWindowsPasswordHash_Invalid_Throws(string? input)
    {
        var user = CreateUser();
        Assert.Throws<ArgumentException>(() => user.ChangeInternalWindowsPasswordHash(input!));
    }

    [Fact]
    public void SetAbsolutePermission_SetsFlag()
    {
        var user = CreateUser(isAbsolute: false);
        user.SetAbsolutePermission(true);
        Assert.True(user.IsAbsolutePermission);
        user.SetAbsolutePermission(false);
        Assert.False(user.IsAbsolutePermission);
    }

    [Fact]
    public void SetPolicy_Valid_AcceptsBoundaries()
    {
        var user = CreateUser();
        user.SetPolicy(0, false);
        Assert.Equal(0, user.DiscountLimitPercent);
        user.SetPolicy(100, true);
        Assert.Equal(100, user.DiscountLimitPercent);
        Assert.True(user.BlockPrintOnRemainingBalance);
        user.SetPolicy(50.5m, false);
        Assert.Equal(50.5m, user.DiscountLimitPercent);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    [InlineData(150)]
    public void SetPolicy_Invalid_Throws(decimal discount)
    {
        var user = CreateUser();
        Assert.Throws<ArgumentException>(() => user.SetPolicy(discount, false));
    }

    [Fact]
    public void SetWorkingHours_BothNull_Clears()
    {
        var user = CreateUser();
        user.SetWorkingHours(new TimeOnly(9, 0), new TimeOnly(17, 0));
        user.SetWorkingHours(null, null);
        Assert.Null(user.WorkStartTime);
        Assert.Null(user.WorkEndTime);
    }

    [Fact]
    public void SetWorkingHours_ValidRange_Sets()
    {
        var user = CreateUser();
        user.SetWorkingHours(new TimeOnly(9, 0), new TimeOnly(17, 0));
        Assert.Equal(new TimeOnly(9, 0), user.WorkStartTime);
        Assert.Equal(new TimeOnly(17, 0), user.WorkEndTime);
    }

    [Fact]
    public void SetWorkingHours_HalfSet_Throws()
    {
        var user = CreateUser();
        Assert.Throws<ArgumentException>(() => user.SetWorkingHours(new TimeOnly(9, 0), null));
        Assert.Throws<ArgumentException>(() => user.SetWorkingHours(null, new TimeOnly(17, 0)));
    }

    [Theory]
    [InlineData(17, 0, 9, 0)]
    [InlineData(9, 0, 9, 0)]
    [InlineData(17, 0, 17, 0)]
    public void SetWorkingHours_StartGreaterOrEqualEnd_Throws(int sh, int sm, int eh, int em)
    {
        var user = CreateUser();
        Assert.Throws<ArgumentException>(() => user.SetWorkingHours(new TimeOnly(sh, sm), new TimeOnly(eh, em)));
    }

    [Fact]
    public void SetBreakPeriod_HasBreakTrue_WithValidDuration_Sets()
    {
        var user = CreateUser();
        user.SetBreakPeriod(true, 60);
        Assert.True(user.HasBreakPeriod);
        Assert.Equal(60, user.BreakDurationMinutes);
    }

    [Fact]
    public void SetBreakPeriod_True_WithNull_Throws()
    {
        var user = CreateUser();
        Assert.Throws<ArgumentException>(() => user.SetBreakPeriod(true, null));
    }

    [Fact]
    public void SetBreakPeriod_True_WithZero_Throws()
    {
        var user = CreateUser();
        Assert.Throws<ArgumentException>(() => user.SetBreakPeriod(true, 0));
        Assert.Throws<ArgumentException>(() => user.SetBreakPeriod(true, -5));
    }

    [Fact]
    public void SetBreakPeriod_False_ClearsDuration()
    {
        var user = CreateUser();
        user.SetBreakPeriod(true, 60);
        user.SetBreakPeriod(false, 60);
        Assert.False(user.HasBreakPeriod);
        Assert.Null(user.BreakDurationMinutes);
        user.SetBreakPeriod(false, null);
        Assert.False(user.HasBreakPeriod);
        Assert.Null(user.BreakDurationMinutes);
    }

    [Fact]
    public void Deactivate_ThenReactivate_RoundTrips()
    {
        var user = CreateUser();
        Assert.True(user.IsActive);
        user.Deactivate();
        Assert.False(user.IsActive);
        user.Reactivate();
        Assert.True(user.IsActive);
    }

    [Fact]
    public void GrantPermission_Twice_SameId_YieldsOneGrant()
    {
        var user = CreateUser();
        var pid = PermissionId.Create(1);
        user.GrantPermission(pid);
        user.GrantPermission(pid);
        Assert.Single(user.PermissionGrants);
        Assert.Equal(pid, user.PermissionGrants.First().PermissionId);
    }

    [Fact]
    public void RevokePermission_NonHeld_IsNoOp()
    {
        var user = CreateUser();
        var pid1 = PermissionId.Create(1);
        var pid2 = PermissionId.Create(2);
        user.GrantPermission(pid1);
        user.RevokePermission(pid2);
        Assert.Single(user.PermissionGrants);
        Assert.Equal(pid1, user.PermissionGrants.First().PermissionId);
    }

    [Fact]
    public void RevokePermission_Held_Removes()
    {
        var user = CreateUser();
        var pid = PermissionId.Create(5);
        user.GrantPermission(pid);
        Assert.Single(user.PermissionGrants);
        user.RevokePermission(pid);
        Assert.Empty(user.PermissionGrants);
    }

    [Fact]
    public void ClearPermissions_EmptiesCollection()
    {
        var user = CreateUser();
        user.GrantPermission(PermissionId.Create(1));
        user.GrantPermission(PermissionId.Create(2));
        user.GrantPermission(PermissionId.Create(3));
        Assert.Equal(3, user.PermissionGrants.Count);
        user.ClearPermissions();
        Assert.Empty(user.PermissionGrants);
    }

    [Fact]
    public void GrantPermission_MultipleDifferentIds_AllAdded()
    {
        var user = CreateUser();
        user.GrantPermission(PermissionId.Create(1));
        user.GrantPermission(PermissionId.Create(2));
        Assert.Equal(2, user.PermissionGrants.Count);
    }
}
