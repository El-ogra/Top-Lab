using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdatePrintResultPassword;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings.Commands;

/// <summary>
/// STAGE-01 (baseline 1.2.2.4.2 / 1.2.2.4.2.1): the print-result password is
/// stored only as a hash; the plain value never reaches the database.
/// </summary>
public class UpdatePrintResultPasswordCommandTests
{
    [Fact]
    public async Task Enable_StoresOnlyTheHash()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        var handler = new UpdatePrintResultPasswordCommandHandler(db);

        var result = await handler.Handle(
            new UpdatePrintResultPasswordCommand(true, "s3cret!"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var settings = Assert.Single(db.SystemSettings);
        Assert.True(settings.PrintResultPasswordEnabled);
        Assert.NotNull(settings.PrintResultPasswordHash);
        Assert.DoesNotContain("s3cret!", settings.PrintResultPasswordHash!, StringComparison.Ordinal);
        Assert.True(PrintResultPasswordHasher.Verify("s3cret!", settings.PrintResultPasswordHash));
    }

    [Fact]
    public async Task Enable_WithoutPassword_Validation()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        var handler = new UpdatePrintResultPasswordCommandHandler(db);

        var result = await handler.Handle(
            new UpdatePrintResultPasswordCommand(true, "   "),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Null(db.SystemSettings[0].PrintResultPasswordHash);
    }

    [Fact]
    public async Task Disable_ClearsTheHash()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        var handler = new UpdatePrintResultPasswordCommandHandler(db);

        await handler.Handle(new UpdatePrintResultPasswordCommand(true, "s3cret!"), CancellationToken.None);
        var result = await handler.Handle(new UpdatePrintResultPasswordCommand(false, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(db.SystemSettings[0].PrintResultPasswordEnabled);
        Assert.Null(db.SystemSettings[0].PrintResultPasswordHash);
    }

    [Fact]
    public async Task MissingSettingsRow_Unexpected()
    {
        var db = new FakeApplicationDbContext();
        var handler = new UpdatePrintResultPasswordCommandHandler(db);

        var result = await handler.Handle(
            new UpdatePrintResultPasswordCommand(true, "s3cret!"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
    }
}
