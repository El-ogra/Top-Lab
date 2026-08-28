using Microsoft.EntityFrameworkCore;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using TopLab.Infrastructure.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence.Interceptors;

public class AuditableInterceptorWithStrongIdTests
{
    [Fact]
    public async Task Added_Patient_WithStrongId_PopulatesAudit()
    {
        var now = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var fakeUser = new FakeCurrentUserService { UserId = 42, IsAuthenticated = true };
        var fakeTime = new FakeDateTimeProvider { UtcNow = now };
        using var ctx = new TestApplicationDbContext(InMemoryContextFactory.Create(fakeUser, fakeTime));
        var patient = Patient.Create(PatientId.Create(0), "Test Patient", TopLab.Domain.Common.Enums.Sex.Male, 30, TopLab.Domain.Common.Enums.AgeUnit.Year, now);
        ctx.Add(patient);
        await ctx.SaveChangesAsync();
        Assert.Equal(42, patient.CreatedByUserId);
        Assert.Equal(now, patient.CreatedAtUtc);
        Assert.Equal(0, patient.ModificationCount);
    }

    [Fact]
    public async Task Modified_Patient_IncrementsCount()
    {
        var now = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var fakeUser = new FakeCurrentUserService { UserId = 1, IsAuthenticated = true };
        var fakeTime = new FakeDateTimeProvider { UtcNow = now };
        using var ctx = new TestApplicationDbContext(InMemoryContextFactory.Create(fakeUser, fakeTime));
        var patient = Patient.Create(PatientId.Create(0), "Ali", TopLab.Domain.Common.Enums.Sex.Male, 20, TopLab.Domain.Common.Enums.AgeUnit.Year, now);
        ctx.Add(patient);
        await ctx.SaveChangesAsync();
        var later = now.AddHours(1);
        fakeTime.UtcNow = later;
        fakeUser.UserId = 99;
        patient.Update("Ali Updated", TopLab.Domain.Common.Enums.Sex.Male, 21, TopLab.Domain.Common.Enums.AgeUnit.Year, null, null, null, false, TopLab.Domain.Common.Enums.AccountType.Individual, null, false, null, false);
        await ctx.SaveChangesAsync();
        Assert.Equal(99, patient.LastModifiedByUserId);
        Assert.Equal(later, patient.LastModifiedAtUtc);
        Assert.Equal(1, patient.ModificationCount);
        Assert.Equal(now, patient.CreatedAtUtc);
    }
}
