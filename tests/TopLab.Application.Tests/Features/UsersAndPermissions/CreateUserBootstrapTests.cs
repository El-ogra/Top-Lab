using TopLab.Application.Features.UsersAndPermissions.Commands.CreateUser;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;

namespace TopLab.Application.Tests.Features.UsersAndPermissions;

public sealed class CreateUserBootstrapTests
{
    private static CreateUserCommand AdminCommand(string name = "firstadmin") => new(
        UserName: name,
        Password: "secret123",
        SecondaryPassword: "sec12345",
        IsAbsolutePermission: true,
        DiscountLimitPercent: 0,
        BlockPrintOnRemainingBalance: false,
        WorkStartTime: null,
        WorkEndTime: null,
        HasBreakPeriod: false,
        BreakDurationMinutes: null,
        PermissionCodes: Array.Empty<string>());

    private static CreateUserCommand RegularCommand(string name = "regular") => new(
        UserName: name,
        Password: "secret123",
        SecondaryPassword: "sec12345",
        IsAbsolutePermission: false,
        DiscountLimitPercent: 0,
        BlockPrintOnRemainingBalance: false,
        WorkStartTime: null,
        WorkEndTime: null,
        HasBreakPeriod: false,
        BreakDurationMinutes: null,
        PermissionCodes: Array.Empty<string>());

    [Fact]
    public async Task Unauthenticated_NoAbsoluteAdmins_AbsoluteRequest_Succeeds()
    {
        var db = new FakeApplicationDbContext();
        var current = new FakeCurrentUserService { IsAuthenticated = false, IsAbsolutePermission = false };
        var handler = new CreateUserCommandHandler(db, new FakePasswordHasher(), current);

        var result = await handler.Handle(AdminCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.Users, u => u.UserName == "firstadmin");
    }

    [Fact]
    public async Task Unauthenticated_NoAbsoluteAdmins_NonAbsoluteRequest_StillForbidden()
    {
        var db = new FakeApplicationDbContext();
        var current = new FakeCurrentUserService { IsAuthenticated = false, IsAbsolutePermission = false };
        var handler = new CreateUserCommandHandler(db, new FakePasswordHasher(), current);

        var result = await handler.Handle(RegularCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Forbidden, result.Error!.Type);
        Assert.Empty(db.Users);
    }

    [Fact]
    public async Task Unauthenticated_OneAbsoluteAdminExists_AbsoluteRequest_Forbidden()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        db.Users.Add(User.Create(UserId.Create(1), "existing", hasher.Hash("p"), hasher.Hash("s"), isAbsolutePermission: true));
        var current = new FakeCurrentUserService { IsAuthenticated = false, IsAbsolutePermission = false };
        var handler = new CreateUserCommandHandler(db, hasher, current);

        var result = await handler.Handle(AdminCommand("second"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Forbidden, result.Error!.Type);
        Assert.Single(db.Users);
    }

    [Fact]
    public async Task AuthenticatedNonAbsolute_AbsoluteRequest_Forbidden()
    {
        var db = new FakeApplicationDbContext();
        var current = new FakeCurrentUserService { IsAuthenticated = true, IsAbsolutePermission = false };
        var handler = new CreateUserCommandHandler(db, new FakePasswordHasher(), current);

        var result = await handler.Handle(AdminCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Forbidden, result.Error!.Type);
    }
}
