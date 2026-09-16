using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.PrintBarcode;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Commands;

public class PrintBarcodeCommandHandlerTests
{
    private sealed class CapturingBarcodeService : IBarcodeService
    {
        public string? LastValue { get; private set; }

        public Result? NextResult { get; set; }

        public Task<Result> PrintBarcodeAsync(string value, CancellationToken cancellationToken = default)
        {
            LastValue = value;
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

    private static (FakeApplicationDbContext Db, FakeSender Sender, CapturingBarcodeService Barcodes) Build(
        bool printLabIdInsteadOfPatientId = false)
    {
        var db = new FakeApplicationDbContext();
        var sender = new FakeSender();
        sender.WithResponse(
            new GetSystemSettingsQuery(),
            Result<SystemSettingsDto>.Success(SettingsDto(printLabIdInsteadOfPatientId)));
        return (db, sender, new CapturingBarcodeService());
    }

    private static Patient PatientWithLabId(int id, string labId)
    {
        return Patient.Create(
            PatientId.Create(id), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow,
            labId: LabId.Create(labId));
    }

    [Fact]
    public async Task PrintBarcode_PatientId_ForwardsPatientId_WhenFlagOff()
    {
        var (db, sender, barcodes) = Build(printLabIdInsteadOfPatientId: false);
        db.Patients.Add(PatientWithLabId(7, "100"));
        var handler = new PrintBarcodeCommandHandler(db, sender, barcodes);

        var result = await handler.Handle(new PrintBarcodeCommand(7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("7", barcodes.LastValue);
    }

    [Fact]
    public async Task PrintBarcode_LabId_ForwardsLabId_WhenFlagOn()
    {
        var (db, sender, barcodes) = Build(printLabIdInsteadOfPatientId: true);
        db.Patients.Add(PatientWithLabId(7, "100"));
        var handler = new PrintBarcodeCommandHandler(db, sender, barcodes);

        var result = await handler.Handle(new PrintBarcodeCommand(7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("100", barcodes.LastValue);
    }

    [Fact]
    public async Task PrintBarcode_LabIdNull_FallsBackToPatientId_WhenFlagOn()
    {
        var (db, sender, barcodes) = Build(printLabIdInsteadOfPatientId: true);
        db.Patients.Add(Patient.Create(PatientId.Create(9), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
        var handler = new PrintBarcodeCommandHandler(db, sender, barcodes);

        var result = await handler.Handle(new PrintBarcodeCommand(9), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("9", barcodes.LastValue);
    }

    [Fact]
    public async Task PrintBarcode_UnknownPatient_NotFound()
    {
        var (db, sender, barcodes) = Build();
        var handler = new PrintBarcodeCommandHandler(db, sender, barcodes);

        var result = await handler.Handle(new PrintBarcodeCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error.Message);
        Assert.Null(barcodes.LastValue);
    }

    [Fact]
    public async Task PrintBarcode_SoftDeletedPatient_NotFound()
    {
        var (db, sender, barcodes) = Build();
        var patient = PatientWithLabId(7, "100");
        patient.SoftDelete();
        db.Patients.Add(patient);
        var handler = new PrintBarcodeCommandHandler(db, sender, barcodes);

        var result = await handler.Handle(new PrintBarcodeCommand(7), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Null(barcodes.LastValue);
    }

    [Fact]
    public async Task PrintBarcode_BarcodeServiceFailure_Propagates()
    {
        var (db, sender, barcodes) = Build();
        db.Patients.Add(PatientWithLabId(7, "100"));
        barcodes.NextResult = Result.Failure(Error.Unexpected("تعذر طباعة الباركود."));
        var handler = new PrintBarcodeCommandHandler(db, sender, barcodes);

        var result = await handler.Handle(new PrintBarcodeCommand(7), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
    }

    [Fact]
    public void PrintBarcode_Requires_AddEditPatient()
    {
        var authorized = (IAuthorizedRequest)new PrintBarcodeCommand(1);

        Assert.Equal("ADD_EDIT_PATIENT", authorized.RequiredPermissionCode);
    }
}

public class PrintBarcodeCommandValidatorTests
{
    private readonly PrintBarcodeCommandValidator _validator = new();

    [Fact]
    public void Validate_ZeroPatientId_Invalid()
    {
        var result = _validator.Validate(new PrintBarcodeCommand(0));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_PositivePatientId_Valid()
    {
        var result = _validator.Validate(new PrintBarcodeCommand(7));

        Assert.True(result.IsValid);
    }
}
