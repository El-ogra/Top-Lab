using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.RemoveTestFromVisit;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Commands;

public class RemoveTestFromVisitCommandHandlerTests
{
    [Fact]
    public async Task Remove_HappyPath()
    {
        var db = new FakeApplicationDbContext();
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(10), 100m));

        var handler = new RemoveTestFromVisitCommandHandler(db);
        var result = await handler.Handle(new RemoveTestFromVisitCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(db.PatientTests);
    }

    [Fact]
    public async Task Remove_Unknown_NotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new RemoveTestFromVisitCommandHandler(db);

        var result = await handler.Handle(new RemoveTestFromVisitCommand(99), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task Remove_TestWithResult_Conflict()
    {
        var db = new FakeApplicationDbContext();
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(10), 100m);
        pt.EnterResult("5.0", ResultFlag.Normal, 1, DateTime.UtcNow);
        db.PatientTests.Add(pt);

        var handler = new RemoveTestFromVisitCommandHandler(db);
        var result = await handler.Handle(new RemoveTestFromVisitCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }
}