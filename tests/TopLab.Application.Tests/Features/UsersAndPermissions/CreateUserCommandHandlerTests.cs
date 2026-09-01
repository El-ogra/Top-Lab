using TopLab.Application.Features.UsersAndPermissions.Commands.CreateUser;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;

namespace TopLab.Application.Tests.Features.UsersAndPermissions;

public class CreateUserCommandHandlerTests
{
    private static void SeedPermissions(FakeApplicationDbContext db)
    {
        var codes = new[]
        {
            "ADD_EDIT_PATIENT", "EDIT_RESULTS", "REVIEW_RESULTS", "PRINT_RESULTS",
            "BLOCK_PRINT_ON_BALANCE", "DELIVER_RESULTS", "DISCOUNT_LIMIT", "PRINT_WORKSHEET",
            "DELETE_PATIENT", "EDIT_SYSTEM_SETTINGS", "CASH_DISBURSE_DEPOSIT", "STATISTICS", "PT_AUDIT_ACCESS"
        };
        for (int i = 0; i < codes.Length; i++)
        {
            db.Permissions.Add(Permission.Create(PermissionId.Create(i + 1), codes[i], $"desc {codes[i]}"));
        }
    }

    [Fact]
    public async Task HappyPath_PersistsUserAndGrants_WithHashedPasswords()
    {
        var db = new FakeApplicationDbContext();
        SeedPermissions(db);
        var hasher = new FakePasswordHasher();
        var handler = new CreateUserCommandHandler(db, hasher);

        var cmd = new CreateUserCommand(
            UserName: "ahmed",
            Password: "secret123",
            SecondaryPassword: "sec12345",
            IsAbsolutePermission: false,
            DiscountLimitPercent: 10,
            BlockPrintOnRemainingBalance: true,
            WorkStartTime: new TimeOnly(9, 0),
            WorkEndTime: new TimeOnly(17, 0),
            HasBreakPeriod: true,
            BreakDurationMinutes: 60,
            PermissionCodes: new[] { "ADD_EDIT_PATIENT", "DISCOUNT_LIMIT", "BLOCK_PRINT_ON_BALANCE" });

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var user = db.Users.First(u => u.UserName == "ahmed");
        Assert.Equal("ahmed", user.UserName);
        Assert.False(user.IsAbsolutePermission);
        Assert.Equal(10, user.DiscountLimitPercent);
        Assert.True(user.BlockPrintOnRemainingBalance);
        Assert.Equal(new TimeOnly(9, 0), user.WorkStartTime);
        Assert.Equal(new TimeOnly(17, 0), user.WorkEndTime);
        Assert.True(user.HasBreakPeriod);
        Assert.Equal(60, user.BreakDurationMinutes);
        Assert.Equal(3, user.PermissionGrants.Count);
        // stored hashes are not plaintext and verify
        Assert.NotEqual("secret123", user.PasswordHash);
        Assert.NotEqual("sec12345", user.InternalWindowsPasswordHash);
        Assert.True(hasher.Verify("secret123", user.PasswordHash));
        Assert.True(hasher.Verify("sec12345", user.InternalWindowsPasswordHash));
    }

    [Fact]
    public async Task DuplicateUserName_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        SeedPermissions(db);
        var hasher = new FakePasswordHasher();
        var handler = new CreateUserCommandHandler(db, hasher);
        var existing = User.Create(UserId.Create(1), "ahmed", hasher.Hash("p1"), hasher.Hash("s1"));
        db.Users.Add(existing);

        var cmd = new CreateUserCommand("ahmed", "secret123", "sec12345", false, 0, false, null, null, false, null, Array.Empty<string>());
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("اسم المستخدم موجود بالفعل", result.Error!.Message);
    }

    [Fact]
    public async Task UnknownPermissionCode_ReturnsValidation()
    {
        var db = new FakeApplicationDbContext();
        SeedPermissions(db);
        var hasher = new FakePasswordHasher();
        var handler = new CreateUserCommandHandler(db, hasher);

        var cmd = new CreateUserCommand("newuser", "secret123", "sec12345", false, 0, false, null, null, false, null, new[] { "UNKNOWN_CODE" });
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public void Validator_RejectsShortPassword()
    {
        var validator = new CreateUserCommandValidator();
        var cmd = new CreateUserCommand("ahmed", "12345", "sec12345", false, 0, false, null, null, false, null, Array.Empty<string>());
        var result = validator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validator_AcceptsSixChars()
    {
        var validator = new CreateUserCommandValidator();
        var cmd = new CreateUserCommand("ahmed", "123456", "123456", false, 0, false, null, null, false, null, Array.Empty<string>());
        var result = validator.Validate(cmd);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validator_RejectsDiscountOver100()
    {
        var validator = new CreateUserCommandValidator();
        var cmd = new CreateUserCommand("ahmed", "secret123", "sec12345", false, 100.5m, false, null, null, false, null, Array.Empty<string>());
        var result = validator.Validate(cmd);
        Assert.False(result.IsValid);
    }
}
