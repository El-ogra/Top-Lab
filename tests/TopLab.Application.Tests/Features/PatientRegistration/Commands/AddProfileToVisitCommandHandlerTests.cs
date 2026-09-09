using TopLab.Application.Features.PatientRegistration.Commands.AddProfileToVisit;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Commands;

public class AddProfileToVisitCommandHandlerTests
{
    private static Patient MakePatient(int id = 1)
    {
        return Patient.Create(PatientId.Create(id), $"P{id}", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
    }

    private static Test MakeSpecializedTest(int id, decimal patientPrice = 500m)
    {
        return Test.Create(TestId.Create(id), $"T{id}", $"T{id}", $"T{id}", $"T{id}", 60, patientPrice, ResultKind.SpecializedProfile);
    }

    [Fact]
    public async Task Order_StoresOnlyFixedPrice_ThroughProfileSelectionCharge()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        db.Tests.Add(MakeSpecializedTest(10, patientPrice: 500m));
        db.Profiles.Add(Profile.Create(ProfileId.Create(20), "Profile", TestId.Create(10), ResultKind.SpecializedProfile, 300m));

        var handler = new AddProfileToVisitCommandHandler(db);
        var result = await handler.Handle(new AddProfileToVisitCommand(1, 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stored = db.PatientTests.Single().PriceAtOrderTime;
        // Constituent/price-list values are irrelevant; profile stores exactly the fixed price.
        Assert.Equal(300m, stored);
        Assert.Equal(PatientAccountCalculator.ProfileSelectionCharge(300m), stored);
    }

    [Fact]
    public async Task Order_MissingProfile_NotFound()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());

        var handler = new AddProfileToVisitCommandHandler(db);
        var result = await handler.Handle(new AddProfileToVisitCommand(1, 999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("البروفايل غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task Order_DeactivatedProfile_Conflict()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        db.Tests.Add(MakeSpecializedTest(10));
        var profile = Profile.Create(ProfileId.Create(20), "Profile", TestId.Create(10), ResultKind.SpecializedProfile, 300m);
        profile.Deactivate();
        db.Profiles.Add(profile);

        var handler = new AddProfileToVisitCommandHandler(db);
        var result = await handler.Handle(new AddProfileToVisitCommand(1, 20), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("البروفايل غير مفعّل.", result.Error!.Message);
    }

    [Fact]
    public async Task Order_NonSpecializedTest_Conflict()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        db.Tests.Add(Test.Create(TestId.Create(10), "T10", "T10", "T10", "T10", 60, 100m, ResultKind.Simple));
        db.Profiles.Add(Profile.Create(ProfileId.Create(20), "Profile", TestId.Create(10), ResultKind.SpecializedProfile, 300m));

        var handler = new AddProfileToVisitCommandHandler(db);
        var result = await handler.Handle(new AddProfileToVisitCommand(1, 20), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("تحليل البروفايل غير صالح.", result.Error!.Message);
    }

    [Fact]
    public async Task Order_MissingPatient_NotFound()
    {
        var db = new FakeApplicationDbContext();

        var handler = new AddProfileToVisitCommandHandler(db);
        var result = await handler.Handle(new AddProfileToVisitCommand(1, 20), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }
}