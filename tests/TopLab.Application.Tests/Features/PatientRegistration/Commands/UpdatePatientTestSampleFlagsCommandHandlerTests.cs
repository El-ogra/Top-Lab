using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.UpdatePatientTestSampleFlags;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Commands;

public class UpdatePatientTestSampleFlagsCommandHandlerTests
{
    [Fact]
    public async Task Update_HappyPath_FlagsPersist()
    {
        var db = new FakeApplicationDbContext();
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(10), 100m);
        db.PatientTests.Add(pt);

        var handler = new UpdatePatientTestSampleFlagsCommandHandler(db);
        var result = await handler.Handle(
            new UpdatePatientTestSampleFlagsCommand(1, true, false, true, false, false, true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(pt.IsUrine);
        Assert.True(pt.IsBlood);
        Assert.True(pt.IsTakenOutsideLab);
    }

    [Fact]
    public async Task Update_Unknown_NotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new UpdatePatientTestSampleFlagsCommandHandler(db);

        var result = await handler.Handle(
            new UpdatePatientTestSampleFlagsCommand(99, true, false, false, false, false, false),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}