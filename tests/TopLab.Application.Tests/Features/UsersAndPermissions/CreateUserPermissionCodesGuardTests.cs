using TopLab.Application.Features.UsersAndPermissions.Commands.CreateUser;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;

namespace TopLab.Application.Tests.Features.UsersAndPermissions;

public sealed class CreateUserPermissionCodesGuardTests
{
    private static FakeApplicationDbContext SeedDb()
    {
        var db = new FakeApplicationDbContext();
        db.Permissions.Add(Permission.Create(PermissionId.Create(1), "ADD_EDIT_PATIENT", "desc"));
        return db;
    }

    private static CreateUserCommand Cmd(bool absolute, IReadOnlyList<string> codes, string name = "u") => new(
        UserName: name,
        Password: "secret123",
        SecondaryPassword: "sec12345",
        IsAbsolutePermission: absolute,
        DiscountLimitPercent: 0,
        BlockPrintOnRemainingBalance: false,
        WorkStartTime: null,
        WorkEndTime: null,
        HasBreakPeriod: false,
        BreakDurationMinutes: null,
        PermissionCodes: codes);

    [Fact]
    public async Task NonAbsoluteCaller_EmptyCodes_NonAbsolute_Succeeds()
    {
        var db = SeedDb();
        var current = new FakeCurrentUserService { IsAuthenticated = true, IsAbsolutePermission = false };
        var result = await new CreateUserCommandHandler(db, new FakePasswordHasher(), current)
            .Handle(Cmd(false, Array.Empty<string>(), "plain"), CancellationToken.None);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task NonAbsoluteCaller_WithCodes_NonAbsolute_Forbidden()
    {
        var db = SeedDb();
        var current = new FakeCurrentUserService { IsAuthenticated = true, IsAbsolutePermission = false };
        var result = await new CreateUserCommandHandler(db, new FakePasswordHasher(), current)
            .Handle(Cmd(false, new[] { "ADD_EDIT_PATIENT" }, "priv"), CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Forbidden, result.Error!.Type);
        Assert.DoesNotContain(db.Users, u => u.UserName == "priv");
    }

    [Fact]
    public async Task AbsoluteCaller_WithCodes_Succeeds()
    {
        var db = SeedDb();
        var current = new FakeCurrentUserService { IsAuthenticated = true, IsAbsolutePermission = true };
        var result = await new CreateUserCommandHandler(db, new FakePasswordHasher(), current)
            .Handle(Cmd(false, new[] { "ADD_EDIT_PATIENT" }, "granted"), CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Single(db.Users, u => u.UserName == "granted");
    }
}
