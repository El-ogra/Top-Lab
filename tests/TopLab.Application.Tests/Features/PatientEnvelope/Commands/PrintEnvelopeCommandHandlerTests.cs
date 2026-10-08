using System.Text.Json;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientEnvelope.Commands.PrintEnvelope;
using TopLab.Application.Features.PatientEnvelope.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientEnvelope.Commands;

public class PrintEnvelopeCommandHandlerTests
{
    private sealed class CapturingEnvelopePrintingService : IEnvelopePrintingService
    {
        public string? LastToken { get; private set; }

        public Result? NextResult { get; set; }

        public Task<Result> PrintEnvelopeAsync(string envelopeToken, CancellationToken cancellationToken = default)
        {
            LastToken = envelopeToken;
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

    private static (FakeApplicationDbContext Db, FakeSender Sender, CapturingEnvelopePrintingService Printing) Build(
        bool printLabIdInsteadOfPatientId = false)
    {
        var db = new FakeApplicationDbContext();
        var sender = new FakeSender();
        sender.WithResponse(
            new GetSystemSettingsQuery(),
            Result<SystemSettingsDto>.Success(SettingsDto(printLabIdInsteadOfPatientId)));
        return (db, sender, new CapturingEnvelopePrintingService());
    }

    private static Patient PatientWithLabId(int id, string labId)
    {
        return Patient.Create(
            PatientId.Create(id), "أحمد محمد", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow,
            labId: LabId.Create(labId));
    }

    private static EnvelopeDto RoundTrip(CapturingEnvelopePrintingService printing)
    {
        Assert.NotNull(printing.LastToken);
        var token = JsonSerializer.Deserialize<EnvelopePrintEnvelope>(printing.LastToken!);
        Assert.NotNull(token);
        var dto = JsonSerializer.Deserialize<EnvelopeDto>(token!.EnvelopeJson);
        Assert.NotNull(dto);
        return dto!;
    }

    [Fact]
    public async Task PrintEnvelope_HappyPath_BuildsTokenFromDtoVerbatim()
    {
        var (db, sender, printing) = Build(printLabIdInsteadOfPatientId: true);
        var patient = PatientWithLabId(7, "100");
        patient.SetReferralEntity(ExternalEntityId.Create(5));
        db.Patients.Add(patient);
        db.ExternalEntities.Add(ExternalEntity.Create(
            ExternalEntityId.Create(5), EntityType.TreatingDoctor, "د. أحمد"));
        var handler = new PrintEnvelopeCommandHandler(db, sender, printing);

        var result = await handler.Handle(new PrintEnvelopeCommand(7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = RoundTrip(printing);
        Assert.Equal(7, dto.PatientId);
        Assert.Equal("أحمد محمد", dto.FullName);
        Assert.Equal("100", dto.Identifier);
        Assert.Equal("د. أحمد", dto.ReferralDisplayName);
        Assert.Equal("100", dto.LabId);
    }

    [Fact]
    public async Task PrintEnvelope_FlagOff_UsesPatientIdAndOmitsMissingReferral()
    {
        var (db, sender, printing) = Build(printLabIdInsteadOfPatientId: false);
        db.Patients.Add(PatientWithLabId(7, "100"));
        var handler = new PrintEnvelopeCommandHandler(db, sender, printing);

        var result = await handler.Handle(new PrintEnvelopeCommand(7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = RoundTrip(printing);
        Assert.Equal("7", dto.Identifier);
        Assert.Null(dto.ReferralDisplayName);
    }

    [Fact]
    public async Task PrintEnvelope_UnknownReferralEntity_OmitsReferralLine()
    {
        var (db, sender, printing) = Build();
        var patient = PatientWithLabId(7, "100");
        patient.SetReferralEntity(ExternalEntityId.Create(999));
        db.Patients.Add(patient);
        var handler = new PrintEnvelopeCommandHandler(db, sender, printing);

        var result = await handler.Handle(new PrintEnvelopeCommand(7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(RoundTrip(printing).ReferralDisplayName);
    }

    [Fact]
    public async Task PrintEnvelope_UnknownPatient_NotFound()
    {
        var (db, sender, printing) = Build();
        var handler = new PrintEnvelopeCommandHandler(db, sender, printing);

        var result = await handler.Handle(new PrintEnvelopeCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error.Message);
        Assert.Null(printing.LastToken);
    }

    [Fact]
    public async Task PrintEnvelope_SoftDeletedPatient_NotFound()
    {
        var (db, sender, printing) = Build();
        var patient = PatientWithLabId(7, "100");
        patient.SoftDelete();
        db.Patients.Add(patient);
        var handler = new PrintEnvelopeCommandHandler(db, sender, printing);

        var result = await handler.Handle(new PrintEnvelopeCommand(7), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Null(printing.LastToken);
    }

    [Fact]
    public async Task PrintEnvelope_SettingsFailure_PropagatesWithoutPrinting()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(PatientWithLabId(7, "100"));
        var sender = new FakeSender();
        sender.WithResponse(
            new GetSystemSettingsQuery(),
            Result<SystemSettingsDto>.Failure(Error.Unexpected("سجل إعدادات النظام مفقود.")));
        var printing = new CapturingEnvelopePrintingService();
        var handler = new PrintEnvelopeCommandHandler(db, sender, printing);

        var result = await handler.Handle(new PrintEnvelopeCommand(7), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
        Assert.Null(printing.LastToken);
    }

    [Fact]
    public async Task PrintEnvelope_PrintingServiceFailure_Propagates()
    {
        var (db, sender, printing) = Build();
        db.Patients.Add(PatientWithLabId(7, "100"));
        printing.NextResult = Result.Failure(Error.Unexpected("تعذر طباعة المظروف."));
        var handler = new PrintEnvelopeCommandHandler(db, sender, printing);

        var result = await handler.Handle(new PrintEnvelopeCommand(7), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
    }

    [Fact]
    public void PrintEnvelope_Requires_PrintResults()
    {
        var authorized = (IAuthorizedRequest)new PrintEnvelopeCommand(7);

        Assert.Equal("PRINT_RESULTS", authorized.RequiredPermissionCode);
    }
}

public class PrintEnvelopeCommandValidatorTests
{
    private readonly PrintEnvelopeCommandValidator _validator = new();

    [Fact]
    public void Validate_ZeroPatientId_Invalid()
    {
        var result = _validator.Validate(new PrintEnvelopeCommand(0));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_PositivePatientId_Valid()
    {
        var result = _validator.Validate(new PrintEnvelopeCommand(7));

        Assert.True(result.IsValid);
    }
}
