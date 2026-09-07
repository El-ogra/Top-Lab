using Microsoft.EntityFrameworkCore;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence;

public class AddTestsToVisitPersistenceTests
{
    [Fact]
    public async Task PatientTest_RoundTrips_PriceAtOrderTime_FromPriceList()
    {
        var options = InMemoryContextFactory.Create();
        await using var ctx = new ApplicationDbContext(options);

        ctx.ExternalEntities.Add(ExternalEntity.Create(
            ExternalEntityId.Create(1),
            EntityType.ReferralOrContract,
            "Alpha Lab",
            priceListId: PriceListId.Create(1),
            generatedIdCode: "AL-1"));

        ctx.PriceLists.Add(PriceList.Create(PriceListId.Create(1), "Standard"));
        ctx.Tests.Add(Test.Create(
            TestId.Create(10),
            "CBC", "CBC", "CBC", "CBC01",
            60,
            patientPrice: 100m,
            labToLabPrice: 50m));

        await ctx.SaveChangesAsync();

        ctx.PriceListItems.Add(new PriceListItem(PriceListId.Create(1), TestId.Create(10), 75m));
        await ctx.SaveChangesAsync();

        var patient = Patient.Create(
            PatientId.Create(1),
            "Alpha Patient",
            Sex.Male, 30, AgeUnit.Year,
            DateTime.UtcNow,
            accountType: AccountType.Contracts);
        patient.SetReferralEntity(ExternalEntityId.Create(1));
        await ctx.AddAsync(patient);
        await ctx.SaveChangesAsync();

        ctx.PatientTests.Add(PatientTest.Create(
            PatientTestId.Create(1),
            PatientId.Create(1),
            TestId.Create(10),
            priceAtOrderTime: 75m));
        await ctx.SaveChangesAsync();

        var saved = await ctx.PatientTests.AsNoTracking().SingleAsync();
        Assert.Equal(75m, saved.PriceAtOrderTime);
        Assert.Equal(1, saved.PatientId.Value);
        Assert.Equal(10, saved.TestId.Value);
    }
}