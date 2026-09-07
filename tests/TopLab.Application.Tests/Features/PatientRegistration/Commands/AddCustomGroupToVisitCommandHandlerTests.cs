using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.AddCustomGroupToVisit;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Commands;

public class AddCustomGroupToVisitCommandHandlerTests
{
    [Fact]
    public async Task Add_HappyPath_ThreeTestsAdded()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
        db.Tests.Add(Test.Create(TestId.Create(10), "T10", "T10", "T10", "T-10", 60, 100m));
        db.Tests.Add(Test.Create(TestId.Create(11), "T11", "T11", "T11", "T-11", 60, 200m));
        db.Tests.Add(Test.Create(TestId.Create(12), "T12", "T12", "T12", "T-12", 60, 300m));

        var sender = new FakeSender()
            .WithCustomGroup(1, new CustomGroupDetailDto(1, "Group", new[]
            {
                new CustomGroupItemDto(10, "T10", "T-10", 80m),
                new CustomGroupItemDto(11, "T11", "T-11", 150m),
                new CustomGroupItemDto(12, "T12", "T-12", 250m)
            }));

        var handler = new AddCustomGroupToVisitCommandHandler(db, sender);
        var result = await handler.Handle(new AddCustomGroupToVisitCommand(1, 1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, db.PatientTests.Count);
        Assert.Equal(80m, db.PatientTests.Single(pt => pt.TestId.Value == 10).PriceAtOrderTime);
        Assert.Equal(150m, db.PatientTests.Single(pt => pt.TestId.Value == 11).PriceAtOrderTime);
        Assert.Equal(250m, db.PatientTests.Single(pt => pt.TestId.Value == 12).PriceAtOrderTime);
    }

    [Fact]
    public async Task Add_UnknownCustomGroup_NotFound()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));

        var handler = new AddCustomGroupToVisitCommandHandler(db, new FakeSender().WithCustomGroupNotFound(99));

        var result = await handler.Handle(new AddCustomGroupToVisitCommand(1, 99), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task Add_SoftDeletedPatient_Conflict()
    {
        var db = new FakeApplicationDbContext();
        var p = Patient.Create(PatientId.Create(1), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.SoftDelete();
        db.Patients.Add(p);

        var handler = new AddCustomGroupToVisitCommandHandler(db, new FakeSender());
        var result = await handler.Handle(new AddCustomGroupToVisitCommand(1, 1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public async Task Add_EmptyGroup_Conflict()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));

        var sender = new FakeSender()
            .WithCustomGroup(1, new CustomGroupDetailDto(1, "Empty", Array.Empty<CustomGroupItemDto>()));

        var handler = new AddCustomGroupToVisitCommandHandler(db, sender);
        var result = await handler.Handle(new AddCustomGroupToVisitCommand(1, 1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }
}