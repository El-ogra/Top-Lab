using System.Text.Json;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientEnvelope.Commands.PrintLabOrder;
using TopLab.Application.Features.PatientEnvelope.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientEnvelope.Commands;

public class PrintLabOrderCommandHandlerTests
{
    private sealed class CapturingLabOrderPrintingService : ILabOrderPrintingService
    {
        public string? LastToken { get; private set; }

        public Result? NextResult { get; set; }

        public Task<Result> PrintLabOrderAsync(string labOrderToken, CancellationToken cancellationToken = default)
        {
            LastToken = labOrderToken;
            return Task.FromResult(NextResult ?? Result.Success());
        }
    }

    private static SystemSettingsDto SettingsDto(bool printLabIdInsteadOfPatientId)
    {
        return new SystemSettingsDto(
            AccountType.Individual, false, false, false, false, false,
            printLabIdInsteadOfPatientId, false, false,
            ResultScreenAccountDisplayMode.Hidden, false, null);
    }

    private static (FakeApplicationDbContext Db, FakeSender Sender, CapturingLabOrderPrintingService Printing) Build(
        bool printLabIdInsteadOfPatientId = false)
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(
            PatientId.Create(7), "أحمد محمد", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow,
            labId: LabId.Create("100")));
        db.Tests.Add(Test.Create(TestId.Create(10), "صورة دم كاملة", "CBC", "CBC", "CBC", 30, 100m));
        db.Tests.Add(Test.Create(TestId.Create(11), "Glucose", "GLU", "Glucose", "GLU", 30, 50m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(100), PatientId.Create(7), TestId.Create(10), 100m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(101), PatientId.Create(7), TestId.Create(11), 50m));

        var sender = new FakeSender();
        sender.WithResponse(
            new GetSystemSettingsQuery(),
            Result<SystemSettingsDto>.Success(SettingsDto(printLabIdInsteadOfPatientId)));
        return (db, sender, new CapturingLabOrderPrintingService());
    }

    private static LabOrderDto RoundTrip(CapturingLabOrderPrintingService printing)
    {
        Assert.NotNull(printing.LastToken);
        var token = JsonSerializer.Deserialize<LabOrderPrintEnvelope>(printing.LastToken!);
        Assert.NotNull(token);
        var dto = JsonSerializer.Deserialize<LabOrderDto>(token!.LabOrderJson);
        Assert.NotNull(dto);
        return dto!;
    }

    [Fact]
    public async Task PrintLabOrder_HappyPath_ListsOrderedTestsWithBarcodeIdentifier()
    {
        var (db, sender, printing) = Build(printLabIdInsteadOfPatientId: true);
        var handler = new PrintLabOrderCommandHandler(db, sender, printing);

        var result = await handler.Handle(new PrintLabOrderCommand(7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = RoundTrip(printing);
        Assert.Equal(7, dto.PatientId);
        Assert.Equal("أحمد محمد", dto.FullName);
        Assert.Equal("100", dto.Identifier);
        Assert.Equal(2, dto.Lines.Count);
        Assert.Equal("CBC", dto.Lines[0].TestCode);
        Assert.Equal("صورة دم كاملة", dto.Lines[0].TestName);
        Assert.Equal("GLU", dto.Lines[1].TestCode);
    }

    [Fact]
    public async Task PrintLabOrder_FlagOff_UsesPatientId()
    {
        var (db, sender, printing) = Build(printLabIdInsteadOfPatientId: false);
        var handler = new PrintLabOrderCommandHandler(db, sender, printing);

        var result = await handler.Handle(new PrintLabOrderCommand(7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("7", RoundTrip(printing).Identifier);
    }

    [Fact]
    public async Task PrintLabOrder_EmptyOrderList_StillPrintsPatientBlockWithBarcode()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(7), "أحمد محمد", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
        var sender = new FakeSender();
        sender.WithResponse(new GetSystemSettingsQuery(), Result<SystemSettingsDto>.Success(SettingsDto(false)));
        var printing = new CapturingLabOrderPrintingService();
        var handler = new PrintLabOrderCommandHandler(db, sender, printing);

        var result = await handler.Handle(new PrintLabOrderCommand(7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = RoundTrip(printing);
        Assert.Empty(dto.Lines);
        Assert.Equal("7", dto.Identifier);
    }

    [Fact]
    public async Task PrintLabOrder_UnknownPatient_NotFound()
    {
        var (db, sender, printing) = Build();
        var handler = new PrintLabOrderCommandHandler(db, sender, printing);

        var result = await handler.Handle(new PrintLabOrderCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error.Message);
        Assert.Null(printing.LastToken);
    }

    [Fact]
    public async Task PrintLabOrder_SoftDeletedPatient_NotFound()
    {
        var (db, sender, printing) = Build();
        db.Patients.Single(p => p.Id.Value == 7).SoftDelete();
        var handler = new PrintLabOrderCommandHandler(db, sender, printing);

        var result = await handler.Handle(new PrintLabOrderCommand(7), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Null(printing.LastToken);
    }

    [Fact]
    public async Task PrintLabOrder_SettingsFailure_PropagatesWithoutPrinting()
    {
        var (db, _, printing) = Build();
        var sender = new FakeSender();
        sender.WithResponse(
            new GetSystemSettingsQuery(),
            Result<SystemSettingsDto>.Failure(Error.Unexpected("سجل إعدادات النظام مفقود.")));
        var handler = new PrintLabOrderCommandHandler(db, sender, printing);

        var result = await handler.Handle(new PrintLabOrderCommand(7), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
        Assert.Null(printing.LastToken);
    }

    [Fact]
    public async Task PrintLabOrder_PrintingServiceFailure_Propagates()
    {
        var (db, sender, printing) = Build();
        printing.NextResult = Result.Failure(Error.Unexpected("تعذر طباعة طلب التحاليل."));
        var handler = new PrintLabOrderCommandHandler(db, sender, printing);

        var result = await handler.Handle(new PrintLabOrderCommand(7), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
    }

    [Fact]
    public void PrintLabOrder_Requires_AddEditPatient()
    {
        var authorized = (IAuthorizedRequest)new PrintLabOrderCommand(7);

        Assert.Equal("ADD_EDIT_PATIENT", authorized.RequiredPermissionCode);
    }
}

public class PrintLabOrderCommandValidatorTests
{
    private readonly PrintLabOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_ZeroPatientId_Invalid()
    {
        var result = _validator.Validate(new PrintLabOrderCommand(0));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_PositivePatientId_Valid()
    {
        var result = _validator.Validate(new PrintLabOrderCommand(7));

        Assert.True(result.IsValid);
    }
}
