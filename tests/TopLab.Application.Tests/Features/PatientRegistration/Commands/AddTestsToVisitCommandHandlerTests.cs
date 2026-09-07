using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.AddTestsToVisit;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Commands;

public class AddTestsToVisitCommandHandlerTests
{
    private static Test MakeTest(int id, decimal patientPrice = 100m, decimal? labToLabPrice = null)
    {
        return Test.Create(
            TestId.Create(id),
            $"Test{id}",
            $"Test{id}",
            $"Test{id}",
            $"T{id}",
            60,
            patientPrice,
            labToLabPrice: labToLabPrice);
    }

    private static Patient MakePatient(int id, AccountType account = AccountType.Individual, int? referralEntityId = null)
    {
        var p = Patient.Create(PatientId.Create(id), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow, accountType: account);
        if (referralEntityId.HasValue)
        {
            p.SetReferralEntity(ExternalEntityId.Create(referralEntityId.Value));
        }
        return p;
    }

    [Fact]
    public async Task Add_CashAccount_UsesPatientPrice()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, AccountType.Individual));
        db.Tests.Add(MakeTest(10, patientPrice: 100m));

        var sender = new FakeSender();
        var handler = new AddTestsToVisitCommandHandler(db, sender);
        var cmd = new AddTestsToVisitCommand(1, new[]
        {
            new AddTestInput(10, false, false, true, false, false, false)
        });

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(100m, db.PatientTests.Single().PriceAtOrderTime);
    }

    [Fact]
    public async Task Add_LabToLab_UsesLabToLabPrice()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, AccountType.LabToLab));
        db.Tests.Add(MakeTest(10, patientPrice: 100m, labToLabPrice: 50m));

        var handler = new AddTestsToVisitCommandHandler(db, new FakeSender());
        var cmd = new AddTestsToVisitCommand(1, new[] { new AddTestInput(10, false, false, true, false, false, false) });

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(50m, db.PatientTests.Single().PriceAtOrderTime);
    }

    [Fact]
    public async Task Add_Contract_WithPriceListHit_UsesPriceListPrice()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, AccountType.Contracts, referralEntityId: 5));
        db.Tests.Add(MakeTest(10, patientPrice: 100m));
        db.ExternalEntities.Add(ExternalEntity.Create(ExternalEntityId.Create(5), EntityType.ReferralOrContract, "Alpha", priceListId: PriceListId.Create(1), generatedIdCode: "AL-1"));

        var sender = new FakeSender()
            .WithPriceList(1, new PriceListDetailDto(1, "Standard", new[]
            {
                new PriceListItemDto(10, "Test10", "T10", 75m)
            }));

        var handler = new AddTestsToVisitCommandHandler(db, sender);
        var cmd = new AddTestsToVisitCommand(1, new[] { new AddTestInput(10, false, false, true, false, false, false) });

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(75m, db.PatientTests.Single().PriceAtOrderTime);
    }

    [Fact]
    public async Task Add_Contract_PriceListMissingTest_Conflict()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, AccountType.Contracts, referralEntityId: 5));
        db.Tests.Add(MakeTest(10));
        db.ExternalEntities.Add(ExternalEntity.Create(ExternalEntityId.Create(5), EntityType.ReferralOrContract, "Alpha", priceListId: PriceListId.Create(1), generatedIdCode: "AL-1"));

        var sender = new FakeSender()
            .WithPriceList(1, new PriceListDetailDto(1, "Standard", Array.Empty<PriceListItemDto>()));

        var handler = new AddTestsToVisitCommandHandler(db, sender);
        var cmd = new AddTestsToVisitCommand(1, new[] { new AddTestInput(10, false, false, true, false, false, false) });

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public async Task Add_UnknownTestId_NotFound()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));

        var handler = new AddTestsToVisitCommandHandler(db, new FakeSender());
        var cmd = new AddTestsToVisitCommand(1, new[] { new AddTestInput(99, false, false, true, false, false, false) });

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task Add_SoftDeletedPatient_Conflict()
    {
        var db = new FakeApplicationDbContext();
        var p = MakePatient(1);
        p.SoftDelete();
        db.Patients.Add(p);

        var handler = new AddTestsToVisitCommandHandler(db, new FakeSender());
        var cmd = new AddTestsToVisitCommand(1, new[] { new AddTestInput(10, false, false, true, false, false, false) });

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public async Task Add_DuplicateTestId_Validation()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        db.Tests.Add(MakeTest(10));

        var handler = new AddTestsToVisitCommandHandler(db, new FakeSender());
        var cmd = new AddTestsToVisitCommand(1, new[]
        {
            new AddTestInput(10, false, false, true, false, false, false),
            new AddTestInput(10, true, false, false, false, false, false)
        });

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public async Task Add_SampleFlags_RoundTrip()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        db.Tests.Add(MakeTest(10));

        var handler = new AddTestsToVisitCommandHandler(db, new FakeSender());
        var cmd = new AddTestsToVisitCommand(1, new[]
        {
            new AddTestInput(10, true, false, true, false, false, true)
        });

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var pt = db.PatientTests.Single();
        Assert.True(pt.IsUrine);
        Assert.True(pt.IsBlood);
        Assert.True(pt.IsTakenOutsideLab);
    }
}