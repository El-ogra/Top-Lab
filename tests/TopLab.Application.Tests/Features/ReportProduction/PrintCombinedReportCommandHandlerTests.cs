using System.Text.Json;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Commands.BuildCombinedReport;
using TopLab.Application.Features.ReportProduction.Commands.PrintCombinedReport;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Application.Tests.Features.ProfileResults;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Users;
using Xunit;

namespace TopLab.Application.Tests.Features.ReportProduction;

public class PrintCombinedReportCommandHandlerTests
{
    private static PatientTest ReviewedRow(int patientTestId, decimal price = 0m)
    {
        var row = PatientTest.Create(PatientTestId.Create(patientTestId), PatientId.Create(1), TestId.Create(2), price);
        row.EnterResult("5.5", ResultFlag.Normal, 1, DateTime.UtcNow);
        row.MarkReviewed(2, DateTime.UtcNow);
        return row;
    }

    private static (FakeApplicationDbContext Db, FakeSender Sender, FakeReportPrintingService Printing, FakeCurrentUserService User, FakeDateTimeProvider Clock) Build(
        decimal price = 0m)
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(ProfileResultsSeed.MakePatient(1));
        db.Users.Add(User.Create(UserId.Create(1), "cashier", "h", "h2", false, 0, false));
        db.PatientTests.Add(ReviewedRow(101, price));

        var sender = new FakeSender();
        sender.WithResponse(
            new BuildCombinedReportCommand(1, new[] { 101 }),
            Result<CombinedReportDto>.Success(new CombinedReportDto(
                1,
                "Ali",
                "LAB-1",
                new List<CombinedReportLineDto>
                {
                    new(101, 2, "Glucose", "GLU", 0, "5.5", 0, "4-6", new List<ProfileReportLineDto>(), null)
                })));

        var printing = new FakeReportPrintingService();
        var user = new FakeCurrentUserService { UserId = 1 };
        return (db, sender, printing, user, new FakeDateTimeProvider());
    }

    [Fact]
    public async Task Print_HappyPath_PrintsAndMarksPrinted()
    {
        var (db, sender, printing, user, clock) = Build();

        var result = await new PrintCombinedReportCommandHandler(db, user, clock, sender, printing)
            .Handle(new PrintCombinedReportCommand(1, new[] { 101 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var token = Assert.Single(printing.Tokens);
        var envelope = JsonSerializer.Deserialize<ReportPrintEnvelope>(token);
        Assert.NotNull(envelope);
        Assert.Equal(ReportPrintEnvelope.Combined, envelope.ReportKind);
        var dto = JsonSerializer.Deserialize<CombinedReportDto>(envelope.ReportJson);
        Assert.Equal("Ali", dto!.PatientFullName);

        var row = db.PatientTests.Single(pt => pt.Id.Value == 101);
        Assert.True(row.IsPrinted);
        Assert.Equal(1, row.PrintCount);
        Assert.Equal(1, row.LastPrintedByUserId);
        Assert.Equal(clock.UtcNow, row.LastPrintedAtUtc);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task Print_BlockedByRemainingBalance_ConflictNoPrint()
    {
        var (db, sender, printing, user, clock) = Build(price: 100m);
        db.Users.Add(User.Create(UserId.Create(5), "cashier", "h", "h2", false, 0, true));
        user.UserId = 5;

        var result = await new PrintCombinedReportCommandHandler(db, user, clock, sender, printing)
            .Handle(new PrintCombinedReportCommand(1, new[] { 101 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("يوجد رصيد متبقٍ على حساب المريض؛ لا يمكن الطباعة.", result.Error.Message);
        Assert.Empty(printing.Tokens);
        Assert.False(db.PatientTests.Single().IsPrinted);
        Assert.Equal(0, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task Print_BlockedButAbsolutePermission_BypassesGate()
    {
        var (db, sender, printing, user, clock) = Build(price: 100m);
        db.Users.Add(User.Create(UserId.Create(5), "admin", "h", "h2", true, 0, true));
        user.UserId = 5;
        user.IsAbsolutePermission = true;

        var result = await new PrintCombinedReportCommandHandler(db, user, clock, sender, printing)
            .Handle(new PrintCombinedReportCommand(1, new[] { 101 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(printing.Tokens);
        Assert.True(db.PatientTests.Single().IsPrinted);
    }

    [Fact]
    public async Task Print_PatientNotFound_NotFoundNoPrint()
    {
        var (db, sender, printing, user, clock) = Build();

        var result = await new PrintCombinedReportCommandHandler(db, user, clock, sender, printing)
            .Handle(new PrintCombinedReportCommand(999, new[] { 101 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error.Message);
        Assert.Empty(printing.Tokens);
    }

    [Fact]
    public async Task Print_UserNotFound_NotFoundNoPrint()
    {
        var (db, sender, printing, user, clock) = Build();
        user.UserId = 42;

        var result = await new PrintCombinedReportCommandHandler(db, user, clock, sender, printing)
            .Handle(new PrintCombinedReportCommand(1, new[] { 101 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المستخدم غير موجود.", result.Error.Message);
        Assert.Empty(printing.Tokens);
    }

    [Fact]
    public async Task Print_AssemblerFailure_PassesThroughWithoutPrinting()
    {
        var (db, _, printing, user, clock) = Build();
        var sender = new FakeSender();
        sender.WithResponse(
            new BuildCombinedReportCommand(1, new[] { 101 }),
            Result<CombinedReportDto>.Failure(Error.Conflict("تعذر تكوين التقرير.")));

        var result = await new PrintCombinedReportCommandHandler(db, user, clock, sender, printing)
            .Handle(new PrintCombinedReportCommand(1, new[] { 101 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("تعذر تكوين التقرير.", result.Error.Message);
        Assert.Empty(printing.Tokens);
    }

    [Fact]
    public async Task Print_PortFailure_PassesThroughWithoutMarkPrinted()
    {
        var (db, sender, printing, user, clock) = Build();
        printing.NextResult = Result.Failure(Error.Unexpected("تعذر طباعة التقرير."));

        var result = await new PrintCombinedReportCommandHandler(db, user, clock, sender, printing)
            .Handle(new PrintCombinedReportCommand(1, new[] { 101 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
        Assert.Single(printing.Tokens);
        Assert.False(db.PatientTests.Single().IsPrinted);
        Assert.Equal(0, db.SaveChangesCallCount);
    }
}