using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence;

public class PaymentOperationPersistenceTests
{
    [Fact]
    public async Task CreatePaymentWithDiscount_Void_Reread_PersistsVoid_AndBalanceMatches()
    {
        var options = InMemoryContextFactory.Create();
        using var ctx = new ApplicationDbContext(options);
        ctx.Database.EnsureCreated();
        ctx.Patients.Add(Patient.Create(PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
        ctx.PatientTests.Add(PatientTest.Create(PatientTestId.Create(100), PatientId.Create(1), TestId.Create(10), 100m));
        var operation = PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(1), 200m, 7, DateTime.UtcNow, 20m);
        ctx.PaymentOperations.Add(operation);
        await ctx.SaveChangesAsync();

        var reloaded = ctx.PaymentOperations.Single(o => o.PatientId.Value == 1);
        Assert.Equal(200m, reloaded.Amount);
        Assert.Equal(20m, reloaded.DiscountAmount);
        Assert.False(reloaded.IsVoided);
        Assert.Equal(
            -120m,
            PatientAccountCalculator.Balance(
                new List<decimal> { 100m },
                ctx.PaymentOperations.Where(o => o.PatientId.Value == 1).ToList()));

        reloaded.Void();
        await ctx.SaveChangesAsync();

        using var reread = new ApplicationDbContext(options);
        var rows = reread.PaymentOperations.Where(o => o.PatientId.Value == 1).ToList();
        Assert.True(Assert.Single(rows).IsVoided);
        Assert.Equal(0m, PatientAccountCalculator.TotalPaid(rows));
        Assert.Equal(
            100m,
            PatientAccountCalculator.Balance(
                new List<decimal> { 100m },
                rows));
    }
}
