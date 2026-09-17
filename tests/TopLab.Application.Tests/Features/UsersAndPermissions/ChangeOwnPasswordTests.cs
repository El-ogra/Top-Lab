using TopLab.Application.Features.UsersAndPermissions.Commands.ChangeOwnPassword;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;

namespace TopLab.Application.Tests.Features.UsersAndPermissions;

public class ChangeOwnPasswordCommandHandlerTests
{
    private static (FakeApplicationDbContext Db, FakePasswordHasher Hasher, FakeCurrentUserService CurrentUser, User User) Arrange()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        var user = User.Create(UserId.Create(1), "ahmed", hasher.Hash("oldpass"), hasher.Hash("sec123"));
        db.Users.Add(user);
        var currentUser = new FakeCurrentUserService { IsAuthenticated = true, UserId = 1, UserName = "ahmed" };
        return (db, hasher, currentUser, user);
    }

    [Fact]
    public async Task WrongCurrentPassword_ReturnsFailureWithExactMessage()
    {
        var (db, hasher, currentUser, _) = Arrange();
        var handler = new ChangeOwnPasswordCommandHandler(db, hasher, currentUser);

        var result = await handler.Handle(new ChangeOwnPasswordCommand("wrong", "newpass123", "newpass123"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("كلمة المرور الحالية غير صحيحة", result.Error!.Message);
    }

    [Fact]
    public async Task CorrectCurrentPassword_UpdatesHash()
    {
        var (db, hasher, currentUser, user) = Arrange();
        var handler = new ChangeOwnPasswordCommandHandler(db, hasher, currentUser);

        var result = await handler.Handle(new ChangeOwnPasswordCommand("oldpass", "newpass123", "newpass123"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(hasher.Verify("newpass123", user.PasswordHash));
        Assert.False(hasher.Verify("oldpass", user.PasswordHash));
    }

    [Fact]
    public async Task UnauthenticatedUser_ReturnsNotFound()
    {
        var (db, hasher, _, _) = Arrange();
        var anonymous = new FakeCurrentUserService { IsAuthenticated = false };
        var handler = new ChangeOwnPasswordCommandHandler(db, hasher, anonymous);

        var result = await handler.Handle(new ChangeOwnPasswordCommand("oldpass", "newpass123", "newpass123"), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}

public class ChangeOwnPasswordCommandValidatorTests
{
    private readonly ChangeOwnPasswordCommandValidator _validator = new();

    [Fact]
    public void ShortNewPassword_FailsWithExactMessage()
    {
        var result = _validator.Validate(new ChangeOwnPasswordCommand("oldpass", "123", "123"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "كلمة المرور الجديدة مطلوبة (6 أحرف على الأقل)");
    }

    [Fact]
    public void MismatchedConfirmation_FailsWithExactMessage()
    {
        var result = _validator.Validate(new ChangeOwnPasswordCommand("oldpass", "newpass123", "otherpass123"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "غير متطابقتان");
    }

    [Fact]
    public void NewSameAsCurrent_FailsWithExactMessage()
    {
        var result = _validator.Validate(new ChangeOwnPasswordCommand("oldpass", "oldpass", "oldpass"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "الجديدة يجب أن تختلف عن الحالية");
    }

    [Fact]
    public void ValidCommand_Passes()
    {
        var result = _validator.Validate(new ChangeOwnPasswordCommand("oldpass", "newpass123", "newpass123"));

        Assert.True(result.IsValid);
    }
}
