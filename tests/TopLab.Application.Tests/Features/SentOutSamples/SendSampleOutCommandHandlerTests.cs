using TopLab.Application.Features.SentOutSamples.Commands.SendSampleOut;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.SentOutSamples;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.SentOutSamples;

public class SendSampleOutCommandHandlerTests
{
    private static FakeApplicationDbContext NewDb() => new();

    private static FakeDateTimeProvider NewTime() => new();

    private static Patient SeedPatient(FakeApplicationDbContext db, int id)
    {
        var p = Patient.Create(PatientId.Create(id), $"P{id}", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        db.Patients.Add(p);
        return p;
    }

    private static Test SeedTest(FakeApplicationDbContext db, int id, bool isSentOut, decimal cost = 50m, decimal price = 100m)
    {
        var t = Test.Create(
            TestId.Create(id), $"T{id}", $"T{id}", $"T{id}", $"T{id}",
            30, price, ResultKind.Simple,
            isSentOut: isSentOut, sentOutCostPrice: isSentOut ? cost : null);
        db.Tests.Add(t);
        return t;
    }

    private static PatientTest SeedPatientTest(FakeApplicationDbContext db, int id, int patientId, int testId)
    {
        var pt = PatientTest.Create(PatientTestId.Create(id), PatientId.Create(patientId), TestId.Create(testId), 100m);
        db.PatientTests.Add(pt);
        return pt;
    }

    private static ExternalEntity SeedEntity(FakeApplicationDbContext db, int id, EntityType type)
    {
        var e = ExternalEntity.Create(ExternalEntityId.Create(id), type, $"Lab{id}");
        db.ExternalEntities.Add(e);
        return e;
    }

    private static void SeedDispatchable(FakeApplicationDbContext db, int patientTestId = 1, int labId = 1, bool isSentOut = true)
    {
        SeedPatient(db, 1);
        SeedTest(db, 1, isSentOut);
        SeedPatientTest(db, patientTestId, 1, 1);
        SeedEntity(db, labId, EntityType.PartnerLab);
    }

    private static async Task<(bool ok, string? message)> Dispatch(
        FakeApplicationDbContext db, int patientTestId = 1, int labId = 1,
        decimal? cost = null, decimal? price = null)
    {
        var handler = new SendSampleOutCommandHandler(db, NewTime());
        var result = await handler.Handle(
            new SendSampleOutCommand(patientTestId, labId, cost, price), CancellationToken.None);
        return (result.IsSuccess, result.Error?.Message);
    }

    [Fact]
    public async Task UnknownPatientTest_ReturnsNotFound()
    {
        var (ok, message) = await Dispatch(NewDb());

        Assert.False(ok);
        Assert.Equal("التحليل غير موجود", message);
    }

    [Fact]
    public async Task MissingPatient_ReturnsNotFound()
    {
        var db = NewDb();
        SeedTest(db, 1, true);
        SeedPatientTest(db, 1, 9, 1);
        SeedEntity(db, 1, EntityType.PartnerLab);

        var (ok, message) = await Dispatch(db);

        Assert.False(ok);
        Assert.Equal("المريض غير موجود.", message);
    }

    [Fact]
    public async Task SoftDeletedPatient_ReturnsNotFound()
    {
        var db = NewDb();
        SeedPatient(db, 1).SoftDelete();
        SeedTest(db, 1, true);
        SeedPatientTest(db, 1, 1, 1);
        SeedEntity(db, 1, EntityType.PartnerLab);

        var (ok, message) = await Dispatch(db);

        Assert.False(ok);
        Assert.Equal("المريض غير موجود.", message);
    }

    [Fact]
    public async Task MissingTest_ReturnsNotFound()
    {
        var db = NewDb();
        SeedPatient(db, 1);
        SeedPatientTest(db, 1, 1, 9);
        SeedEntity(db, 1, EntityType.PartnerLab);

        var (ok, message) = await Dispatch(db);

        Assert.False(ok);
        Assert.Equal("التحليل غير موجود", message);
    }

    [Fact]
    public async Task NonSentOutTest_ReturnsConflict()
    {
        var db = NewDb();
        SeedDispatchable(db, isSentOut: false);

        var (ok, message) = await Dispatch(db);

        Assert.False(ok);
        Assert.Equal("هذا التحليل غير مهيأ للإرسال للخارج.", message);
    }

    [Fact]
    public async Task UnknownEntity_ReturnsNotFound()
    {
        var db = NewDb();
        SeedPatient(db, 1);
        SeedTest(db, 1, true);
        SeedPatientTest(db, 1, 1, 1);

        var (ok, message) = await Dispatch(db);

        Assert.False(ok);
        Assert.Equal("الجهة الخارجية غير موجودة.", message);
    }

    [Fact]
    public async Task NonLabEntity_ReturnsConflict()
    {
        var db = NewDb();
        SeedPatient(db, 1);
        SeedTest(db, 1, true);
        SeedPatientTest(db, 1, 1, 1);
        SeedEntity(db, 1, EntityType.TreatingDoctor);

        var (ok, message) = await Dispatch(db);

        Assert.False(ok);
        Assert.Equal("الجهة المختارة ليست معملًا خارجيًا.", message);
    }

    [Fact]
    public async Task DuplicateDispatch_ReturnsConflict()
    {
        var db = NewDb();
        SeedDispatchable(db);

        var first = await Dispatch(db);

        Assert.True(first.ok);

        var (ok, message) = await Dispatch(db);

        Assert.False(ok);
        Assert.Equal("تم إرسال هذه العينة مسبقًا.", message);
    }

    [Fact]
    public async Task OmittedPrices_CapturedFromTestDefinition()
    {
        var db = NewDb();
        var time = NewTime();
        SeedDispatchable(db);

        var handler = new SendSampleOutCommandHandler(db, time);
        var result = await handler.Handle(new SendSampleOutCommand(1, 1, null, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stored = Assert.Single(db.SentOutSamples);
        Assert.Equal(50m, stored.CostPrice);
        Assert.Equal(100m, stored.PatientPrice);
        Assert.Equal(time.UtcNow, stored.SentAtUtc);
        Assert.Equal(1, stored.PatientTestId.Value);
        Assert.Equal(1, stored.ExternalLabEntityId.Value);
    }

    [Fact]
    public async Task ExplicitPrices_OverrideHonored()
    {
        var db = NewDb();
        SeedDispatchable(db);

        var (ok, _) = await Dispatch(db, cost: 70m, price: 150m);

        Assert.True(ok);
        var stored = Assert.Single(db.SentOutSamples);
        Assert.Equal(70m, stored.CostPrice);
        Assert.Equal(150m, stored.PatientPrice);
    }

    [Fact]
    public async Task NegativeOverride_ReturnsValidation()
    {
        var db = NewDb();
        SeedDispatchable(db);

        var handler = new SendSampleOutCommandHandler(db, NewTime());
        var result = await handler.Handle(new SendSampleOutCommand(1, 1, -5m, null), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("السعر يجب ألا يكون سالبًا.", result.Error!.Message);
    }

    [Fact]
    public void Validator_ZeroIds_Rejected()
    {
        var validator = new SendSampleOutCommandValidator();

        Assert.False(validator.Validate(new SendSampleOutCommand(0, 1, null, null)).IsValid);
        Assert.False(validator.Validate(new SendSampleOutCommand(1, 0, null, null)).IsValid);
    }

    [Fact]
    public void Validator_NegativePrice_RejectedWithMessage()
    {
        var validator = new SendSampleOutCommandValidator();
        var result = validator.Validate(new SendSampleOutCommand(1, 1, -1m, -2m));

        Assert.False(result.IsValid);
        Assert.All(result.Errors, e => Assert.Equal("السعر يجب ألا يكون سالبًا.", e.ErrorMessage));
    }

    [Fact]
    public void Validator_Valid_Passes()
    {
        var validator = new SendSampleOutCommandValidator();

        Assert.True(validator.Validate(new SendSampleOutCommand(1, 1, 10m, 20m)).IsValid);
        Assert.True(validator.Validate(new SendSampleOutCommand(1, 1, null, null)).IsValid);
    }
}
