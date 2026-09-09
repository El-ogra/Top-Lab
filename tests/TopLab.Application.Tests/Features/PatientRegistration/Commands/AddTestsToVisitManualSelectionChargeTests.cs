using TopLab.Application.Features.PatientRegistration.Commands.AddTestsToVisit;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Commands;

public class AddTestsToVisitManualSelectionChargeTests
{
    private static Test MakeTest(int id, decimal patientPrice)
    {
        return Test.Create(TestId.Create(id), $"T{id}", $"T{id}", $"T{id}", $"T{id}", 60, patientPrice, ResultKind.Simple);
    }

    private static Patient MakePatient(int id = 1)
    {
        return Patient.Create(PatientId.Create(id), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
    }

    [Fact]
    public async Task ManualSelection_RoutesThroughCentralCalculator_SingleTest()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        db.Tests.Add(MakeTest(10, patientPrice: 100m));

        var handler = new AddTestsToVisitCommandHandler(db, new FakeSender());
        var result = await handler.Handle(
            new AddTestsToVisitCommand(1, new[] { new AddTestInput(10, false, false, true, false, false, false) }),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            PatientAccountCalculator.ManualSelectionCharge(new[] { 100m }),
            db.PatientTests.Single().PriceAtOrderTime);
    }

    [Fact]
    public async Task ManualSelection_StoresSumOfIndividualPrices()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        db.Tests.Add(MakeTest(10, patientPrice: 100m));
        db.Tests.Add(MakeTest(11, patientPrice: 50m));

        var handler = new AddTestsToVisitCommandHandler(db, new FakeSender());
        var result = await handler.Handle(
            new AddTestsToVisitCommand(1, new[]
            {
                new AddTestInput(10, false, false, true, false, false, false),
                new AddTestInput(11, false, false, true, false, false, false),
            }),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            PatientAccountCalculator.ManualSelectionCharge(new[] { 100m, 50m }),
            db.PatientTests.Sum(pt => pt.PriceAtOrderTime));
    }
}