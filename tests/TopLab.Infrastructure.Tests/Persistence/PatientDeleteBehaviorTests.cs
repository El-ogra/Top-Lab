using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence;

public class PatientDeleteBehaviorTests
{
    private static IEntityType EntityTypeOf<T>() where T : class
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        var et = ctx.Model.FindEntityType(typeof(T));
        Assert.NotNull(et);
        return et!;
    }

    private static IForeignKey? FkToPrincipal<TDependent>(System.Type principal)
        where TDependent : class
    {
        return EntityTypeOf<TDependent>().GetForeignKeys()
            .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == principal);
    }

    [Fact]
    public void DeletingPatient_CascadesToPatientPhoneNumber()
    {
        var fk = FkToPrincipal<PatientPhoneNumber>(typeof(Patient));
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
    }

    [Fact]
    public void DeletingPatient_CascadesToPatientMedicalCondition()
    {
        var fk = FkToPrincipal<PatientMedicalCondition>(typeof(Patient));
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
    }

    [Fact]
    public void DeletingPatient_CascadesToPatientTest()
    {
        var fk = FkToPrincipal<PatientTest>(typeof(Patient));
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
    }

    [Fact]
    public void PatientPhoneNumber_HasNoModelForeignKeyToPatientPhoneNumberType_BaselineDocumented()
    {
        var et = EntityTypeOf<PatientPhoneNumber>();
        Assert.DoesNotContain(
            et.GetForeignKeys(),
            fk => fk.PrincipalEntityType.ClrType == typeof(PatientPhoneNumber));
    }

    [Fact]
    public void PatientTest_HasNoModelForeignKeyToTestGroup_BaselineDocumented()
    {
        var et = EntityTypeOf<PatientTest>();
        Assert.DoesNotContain(
            et.GetForeignKeys(),
            fk => fk.PrincipalEntityType.ClrType == typeof(TestGroup));
    }
}