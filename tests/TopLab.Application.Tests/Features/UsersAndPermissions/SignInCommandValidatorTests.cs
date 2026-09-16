using TopLab.Application.Features.UsersAndPermissions.Commands.SignIn;
using Xunit;

namespace TopLab.Application.Tests.Features.UsersAndPermissions;

public class SignInCommandValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SignInValidator_Rejects_NullEmptyWhitespaceUserName_WithExactMessage(string? userName)
    {
        var validator = new SignInCommandValidator();
        var result = validator.Validate(new SignInCommand(userName!, "password123"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "اسم المستخدم مطلوب.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SignInValidator_Rejects_NullEmptyWhitespacePassword_WithExactMessage(string? password)
    {
        var validator = new SignInCommandValidator();
        var result = validator.Validate(new SignInCommand("admin", password!));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "كلمة المرور مطلوبة.");
    }

    [Fact]
    public void SignInValidator_Accepts_ValidInput()
    {
        var validator = new SignInCommandValidator();
        var result = validator.Validate(new SignInCommand("admin", "password123"));

        Assert.True(result.IsValid);
    }
}
