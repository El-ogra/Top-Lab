using TopLab.Application.Features.ReportProduction.Commands.BuildBlankReport;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.ReportProduction;

public class BuildBlankReportCommandHandlerTests
{
    [Fact]
    public async Task ResolvesDoctorAndReferralNames()
    {
        var db = new FakeApplicationDbContext();
        var doctor = ExternalEntity.Create(ExternalEntityId.Create(70), EntityType.TreatingDoctor, "Dr. Hany");
        var referral = ExternalEntity.Create(ExternalEntityId.Create(71), EntityType.ReferralOrContract, "Safwa Lab", priceListId: PriceListId.Create(1));
        db.ExternalEntities.Add(doctor);
        db.ExternalEntities.Add(referral);
        db.Patients.Add(Patient.Create(
            PatientId.Create(1), "Ali", Sex.Male, 44, AgeUnit.Year, DateTime.UtcNow,
            labId: LabId.Create("L-77"),
            treatingDoctorId: ExternalEntityId.Create(70),
            referralEntityId: ExternalEntityId.Create(71)));

        var result = await new BuildBlankReportCommandHandler(db)
            .Handle(new BuildBlankReportCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = result.Value!;
        Assert.Equal("Ali", dto.PatientFullName);
        Assert.Equal("L-77", dto.LabId);
        Assert.Equal("Male", dto.Sex);
        Assert.Equal(44, dto.AgeValue);
        Assert.Equal("Year", dto.AgeUnit);
        Assert.Equal("Dr. Hany", dto.TreatingDoctorName);
        Assert.Equal("Safwa Lab", dto.ReferralEntityName);
    }

    [Fact]
    public async Task UnresolvedDoctorOrReferral_ReturnsNullNames()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 44, AgeUnit.Year, DateTime.UtcNow));

        var result = await new BuildBlankReportCommandHandler(db)
            .Handle(new BuildBlankReportCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.TreatingDoctorName);
        Assert.Null(result.Value.ReferralEntityName);
    }

    [Fact]
    public async Task MissingPatient_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();

        var result = await new BuildBlankReportCommandHandler(db)
            .Handle(new BuildBlankReportCommand(42), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task DeletedPatient_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var patient = Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 44, AgeUnit.Year, DateTime.UtcNow);
        patient.SoftDelete();
        db.Patients.Add(patient);

        var result = await new BuildBlankReportCommandHandler(db)
            .Handle(new BuildBlankReportCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }
}