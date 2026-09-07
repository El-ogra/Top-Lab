using TopLab.Application.Features.PatientRegistration.Queries.GetPatientTitles;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Queries;

public class GetPatientTitlesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsTitlesOrderedById()
    {
        var db = new FakeApplicationDbContext();
        db.PatientTitles.Add(PatientTitle.Create(PatientTitleId.Create(3), "Mr.", true));
        db.PatientTitles.Add(PatientTitle.Create(PatientTitleId.Create(1), "Dr."));
        db.PatientTitles.Add(PatientTitle.Create(PatientTitleId.Create(2), "Mrs."));

        var handler = new GetPatientTitlesQueryHandler(db);
        var result = await handler.Handle(new GetPatientTitlesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Count);
        Assert.Equal(1, result.Value![0].PatientTitleId);
    }

    [Fact]
    public async Task Handle_Empty_ReturnsEmpty()
    {
        var db = new FakeApplicationDbContext();
        var handler = new GetPatientTitlesQueryHandler(db);

        var result = await handler.Handle(new GetPatientTitlesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }
}