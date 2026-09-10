using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence;

/// <summary>
/// M-06 infrastructure proofs on the real <see cref="ApplicationDbContext"/> over the
/// InMemory provider: header upsert + replace-list sensitivities, the cascade graph
/// round-trip (PatientTest -> CultureResult -> CultureAntibioticResult), and the
/// Restrict-on-antibiotic-delete pin asserted at the model level.
/// </summary>
public class CultureResultPersistenceTests
{
    [Fact]
    public async Task UpsertHeader_AndReplaceSensitivities_RoundTrips()
    {
        var options = InMemoryContextFactory.Create();
        await using var ctx = new ApplicationDbContext(options);
        ctx.Database.EnsureCreated();

        ctx.Patients.Add(Patient.Create(PatientId.Create(1), "Culture", Sex.Female, 30, AgeUnit.Year, DateTime.UtcNow));
        ctx.Tests.Add(Test.Create(TestId.Create(10), "Culture", "Culture", "Culture", "C10", 60, 100m, ResultKind.Culture, isCultureType: true));
        ctx.Antibiotics.Add(Antibiotic.Create(AntibioticId.Create(1), "Penicillin"));
        ctx.Antibiotics.Add(Antibiotic.Create(AntibioticId.Create(2), "Ciprofloxacin"));
        await ctx.SaveChangesAsync();

        var pt = PatientTest.Create(PatientTestId.Create(0), PatientId.Create(1), TestId.Create(10), 100m);
        ctx.PatientTests.Add(pt);
        await ctx.SaveChangesAsync();

        var header = new CultureResult(pt.Id, sample: "Urine", organismA: "E. coli", organismB: null, organismC: null, cultureCondition: "37C", colonyCount: "12");
        ctx.CultureResults.Add(header);
        ctx.CultureAntibioticResults.Add(CultureAntibioticResult.Create(CultureAntibioticResultId.Create(1), pt.Id, AntibioticId.Create(1), SensitivityCategory.HighlyFor));
        ctx.CultureAntibioticResults.Add(CultureAntibioticResult.Create(CultureAntibioticResultId.Create(2), pt.Id, AntibioticId.Create(2), SensitivityCategory.ResistantFor));
        await ctx.SaveChangesAsync();

        header.Update("Blood", "S. aureus", organismB: null, organismC: null, "35C", "3");
        var old = await ctx.CultureAntibioticResults.ToListAsync();
        ctx.CultureAntibioticResults.RemoveRange(old);
        ctx.CultureAntibioticResults.Add(CultureAntibioticResult.Create(CultureAntibioticResultId.Create(3), pt.Id, AntibioticId.Create(2), SensitivityCategory.ModerateFor));
        await ctx.SaveChangesAsync();

        await using var reread = new ApplicationDbContext(options);
        var persisted = await reread.CultureResults.AsNoTracking().SingleAsync();
        Assert.Equal("Blood", persisted.Sample);
        Assert.Equal("S. aureus", persisted.OrganismA);
        Assert.Equal("35C", persisted.CultureCondition);
        Assert.Equal("3", persisted.ColonyCount);

        var sensitivity = Assert.Single(await reread.CultureAntibioticResults.AsNoTracking().ToListAsync());
        Assert.Equal(2, sensitivity.AntibioticId.Value);
        Assert.Equal(SensitivityCategory.ModerateFor, sensitivity.SensitivityCategory);
        Assert.Equal(pt.Id.Value, sensitivity.PatientTestId.Value);
    }

    [Fact]
    public async Task DeletingPatientTest_CascadesCultureGraph()
    {
        var options = InMemoryContextFactory.Create();
        await using var ctx = new ApplicationDbContext(options);
        ctx.Database.EnsureCreated();

        ctx.Patients.Add(Patient.Create(PatientId.Create(1), "Culture", Sex.Male, 40, AgeUnit.Year, DateTime.UtcNow));
        ctx.Tests.Add(Test.Create(TestId.Create(11), "Culture", "Culture", "Culture", "C11", 60, 100m, ResultKind.Culture, isCultureType: true));
        ctx.Antibiotics.Add(Antibiotic.Create(AntibioticId.Create(3), "Amoxicillin"));
        await ctx.SaveChangesAsync();

        var pt = PatientTest.Create(PatientTestId.Create(0), PatientId.Create(1), TestId.Create(11), 100m);
        ctx.PatientTests.Add(pt);
        await ctx.SaveChangesAsync();

        ctx.CultureResults.Add(new CultureResult(pt.Id, sample: "Stool", organismA: "Salmonella"));
        ctx.CultureAntibioticResults.Add(CultureAntibioticResult.Create(CultureAntibioticResultId.Create(1), pt.Id, AntibioticId.Create(3), SensitivityCategory.LowFor));
        await ctx.SaveChangesAsync();
        Assert.Single(ctx.CultureResults);
        Assert.Single(ctx.CultureAntibioticResults);

        ctx.PatientTests.Remove(pt);
        await ctx.SaveChangesAsync();

        Assert.Empty(ctx.CultureResults);
        Assert.Empty(ctx.CultureAntibioticResults);
    }

    [Fact]
    public void AntibioticDelete_IsRestricted_AtModelLevel()
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        var sensitivity = ctx.Model.FindEntityType(typeof(CultureAntibioticResult));
        Assert.NotNull(sensitivity);

        var toAntibiotic = sensitivity!.GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Antibiotic));
        Assert.Equal(DeleteBehavior.Restrict, toAntibiotic.DeleteBehavior);

        var toCulture = sensitivity.GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(CultureResult));
        Assert.Equal(DeleteBehavior.Cascade, toCulture.DeleteBehavior);

        var pk = sensitivity.FindPrimaryKey();
        Assert.NotNull(pk);
        Assert.Equal(ValueGenerated.OnAdd, pk!.Properties.Single().ValueGenerated);

        Assert.Contains(sensitivity.GetIndexes(),
            x => x.Properties.Any(p => p.Name == nameof(CultureAntibioticResult.PatientTestId)));

        var headerFk = Assert.Single(ctx.Model.FindEntityType(typeof(CultureResult))!.GetForeignKeys());
        Assert.Equal(DeleteBehavior.Cascade, headerFk.DeleteBehavior);
        Assert.Equal(typeof(PatientTest), headerFk.PrincipalEntityType.ClrType);
    }
}