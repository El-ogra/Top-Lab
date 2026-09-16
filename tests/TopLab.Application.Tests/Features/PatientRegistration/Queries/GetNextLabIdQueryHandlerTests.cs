using TopLab.Application.Features.PatientRegistration.Queries.GetNextLabId;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Queries;

public class GetNextLabIdQueryHandlerTests
{
    private static void AddPatient(FakeApplicationDbContext db, int id, string? labId)
    {
        db.Patients.Add(Patient.Create(
            PatientId.Create(id), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow,
            labId: labId is null ? null : LabId.Create(labId)));
    }

    [Fact]
    public async Task EmptyDb_ReturnsOne()
    {
        var db = new FakeApplicationDbContext();
        var handler = new GetNextLabIdQueryHandler(db);

        var result = await handler.Handle(new GetNextLabIdQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("1", result.Value);
    }

    [Fact]
    public async Task NonEmptyDb_ReturnsMaxPlusOne()
    {
        var db = new FakeApplicationDbContext();
        AddPatient(db, 1, "7");
        AddPatient(db, 2, "100");
        AddPatient(db, 3, null);
        var handler = new GetNextLabIdQueryHandler(db);

        var result = await handler.Handle(new GetNextLabIdQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("101", result.Value);
    }

    [Fact]
    public async Task NonNumericLabIds_SkippedDeterministically()
    {
        var db = new FakeApplicationDbContext();
        AddPatient(db, 1, "LAB-X");
        AddPatient(db, 2, "ABC");
        AddPatient(db, 3, "42");
        var handler = new GetNextLabIdQueryHandler(db);

        var result = await handler.Handle(new GetNextLabIdQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("43", result.Value);
    }

    [Fact]
    public async Task PaddedWidth_Preserved()
    {
        var db = new FakeApplicationDbContext();
        AddPatient(db, 1, "7");
        AddPatient(db, 2, "0042");
        var handler = new GetNextLabIdQueryHandler(db);

        var result = await handler.Handle(new GetNextLabIdQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("0043", result.Value);
    }

    [Fact]
    public async Task OnlyNonNumericLabIds_ReturnsOne()
    {
        var db = new FakeApplicationDbContext();
        AddPatient(db, 1, "LAB-X");
        var handler = new GetNextLabIdQueryHandler(db);

        var result = await handler.Handle(new GetNextLabIdQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("1", result.Value);
    }
}
