using TopLab.Application.Features.ResultDelivery.Queries.GetDeliveryAccount;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultDelivery;

public class GetDeliveryAccountNotesTests
{
    [Fact]
    public async Task CarriesNotes_WhenPresent()
    {
        var db = new FakeApplicationDbContext();
        var patient = Patient.Create(PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow, notes: "note text");
        db.Patients.Add(patient);
        var handler = new GetDeliveryAccountQueryHandler(db);
        var result = await handler.Handle(new GetDeliveryAccountQuery(1), CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal("note text", result.Value!.PatientNotes);
    }

    [Fact]
    public async Task CarriesNull_WhenAbsent()
    {
        var db = new FakeApplicationDbContext();
        var patient = Patient.Create(PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        db.Patients.Add(patient);
        var handler = new GetDeliveryAccountQueryHandler(db);
        var result = await handler.Handle(new GetDeliveryAccountQuery(1), CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.PatientNotes);
    }
}
