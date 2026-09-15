using System.Text.Json;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Commands.BuildBlankReport;
using TopLab.Application.Features.ReportProduction.Commands.PrintBlankReport;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Application.Tests.Features.ProfileResults;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;
using Xunit;

namespace TopLab.Application.Tests.Features.ReportProduction;

public class PrintBlankReportCommandHandlerTests
{
    private static (FakeApplicationDbContext Db, FakeSender Sender, FakeReportPrintingService Printing, FakeCurrentUserService User) Build()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(ProfileResultsSeed.MakePatient(1));
        db.Users.Add(User.Create(UserId.Create(1), "cashier", "h", "h2", false, 0, false));

        var sender = new FakeSender();
        sender.WithResponse(
            new BuildBlankReportCommand(1),
            Result<BlankReportDto>.Success(new BlankReportDto(1, "Ali", "LAB-1", "Male", 30, "Year", "Dr.Hassan", "Al-Shifa")));

        var printing = new FakeReportPrintingService();
        return (db, sender, printing, new FakeCurrentUserService { UserId = 1 });
    }

    [Fact]
    public async Task Print_HappyPath_PrintsBlankReport()
    {
        var (db, sender, printing, user) = Build();

        var result = await new PrintBlankReportCommandHandler(db, user, sender, printing)
            .Handle(new PrintBlankReportCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var token = Assert.Single(printing.Tokens);
        var envelope = JsonSerializer.Deserialize<ReportPrintEnvelope>(token);
        Assert.NotNull(envelope);
        Assert.Equal(ReportPrintEnvelope.Blank, envelope.ReportKind);
        Assert.Equal("Ali", JsonSerializer.Deserialize<BlankReportDto>(envelope.ReportJson)!.PatientFullName);
    }

    [Fact]
    public async Task Print_BlockingUserPositiveBalanceStillPrints_BlankIsExempt()
    {
        var (db, sender, printing, user) = Build();
        db.Users.Add(User.Create(UserId.Create(5), "cashier", "h", "h2", false, 0, true));
        user.UserId = 5;

        var result = await new PrintBlankReportCommandHandler(db, user, sender, printing)
            .Handle(new PrintBlankReportCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(printing.Tokens);
    }

    [Fact]
    public async Task Print_PatientNotFound_NotFoundNoPrint()
    {
        var (db, sender, printing, user) = Build();

        var result = await new PrintBlankReportCommandHandler(db, user, sender, printing)
            .Handle(new PrintBlankReportCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error.Message);
        Assert.Empty(printing.Tokens);
    }

    [Fact]
    public async Task Print_UserNotFound_NotFoundNoPrint()
    {
        var (db, sender, printing, user) = Build();
        user.UserId = 42;

        var result = await new PrintBlankReportCommandHandler(db, user, sender, printing)
            .Handle(new PrintBlankReportCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المستخدم غير موجود.", result.Error.Message);
        Assert.Empty(printing.Tokens);
    }

    [Fact]
    public async Task Print_AssemblerFailure_PassesThrough()
    {
        var (db, _, printing, user) = Build();
        var sender = new FakeSender();
        sender.WithResponse(
            new BuildBlankReportCommand(1),
            Result<BlankReportDto>.Failure(Error.Conflict("تعذر تكوين التقرير.")));

        var result = await new PrintBlankReportCommandHandler(db, user, sender, printing)
            .Handle(new PrintBlankReportCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("تعذر تكوين التقرير.", result.Error!.Message);
        Assert.Empty(printing.Tokens);
    }

    [Fact]
    public async Task Print_PortFailure_PassesThrough()
    {
        var (db, sender, printing, user) = Build();
        printing.NextResult = Result.Failure(Error.Unexpected("تعذر طباعة التقرير."));

        var result = await new PrintBlankReportCommandHandler(db, user, sender, printing)
            .Handle(new PrintBlankReportCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
        Assert.Single(printing.Tokens);
    }
}