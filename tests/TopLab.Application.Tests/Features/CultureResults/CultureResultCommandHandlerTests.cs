using TopLab.Application.Features.CultureResults.Commands.SaveCultureResults;using TopLab.Application.Features.CultureResults.Commands.VerifyCultureResult;using TopLab.Application.Tests.Common.Fakes;using TopLab.Domain.Common.Enums;using TopLab.Domain.Common.Ids;using TopLab.Domain.Patients;using TopLab.Domain.Results;using TopLab.Domain.Tests;using Xunit;
namespace TopLab.Application.Tests.Features.CultureResults;
public class CultureResultCommandHandlerTests{
[Fact]public async Task Save_RejectsAntibioticNotAttached(){var db=Seed();var r=await new SaveCultureResultsCommandHandler(db).Handle(new SaveCultureResultsCommand(10,null,null,null,null,null,null,[new(99,0)]),default);Assert.False(r.IsSuccess);Assert.Equal("المضاد الحيوي غير مرفق بهذه المزرعة.",r.Error!.Message);}
[Fact]public async Task Save_ReplaceListAndVerify(){var db=Seed();var save=new SaveCultureResultsCommandHandler(db);Assert.True((await save.Handle(new SaveCultureResultsCommand(10," sample ",null,null,null,null,null,[new(1,0)]),default)).IsSuccess);Assert.Single(db.CultureAntibioticResults);var verify=await new VerifyCultureResultCommandHandler(db,new FakeCurrentUserService(),new FakeDateTimeProvider()).Handle(new VerifyCultureResultCommand(10),default);Assert.True(verify.IsSuccess);Assert.True(db.PatientTests.Single().IsReviewed);}

// ---- W-02 Slice 1 / C-19: a null SensitivityCategory must reach the store as NULL ----

[Fact]public async Task SaveCulture_PersistsUnspecifiedAsNull()
{
    var db=Seed();
    var r=await new SaveCultureResultsCommandHandler(db).Handle(
        new SaveCultureResultsCommand(10," sample ",null,null,null,null,null,[new(1,null)]),default);

    Assert.True(r.IsSuccess);
    var row=Assert.Single(db.CultureAntibioticResults);
    Assert.Null(row.SensitivityCategory);
}

[Fact]public async Task SaveCulture_PersistsSensitiveAsHighlyFor()
{
    var db=Seed();
    var r=await new SaveCultureResultsCommandHandler(db).Handle(
        new SaveCultureResultsCommand(10,null,null,null,null,null,null,[new(1,(int)SensitivityCategory.HighlyFor)]),default);

    Assert.True(r.IsSuccess);
    Assert.Equal(SensitivityCategory.HighlyFor,Assert.Single(db.CultureAntibioticResults).SensitivityCategory);
}

[Fact]public async Task SaveCulture_PersistsIntermediateAsModerateFor()
{
    var db=Seed();
    var r=await new SaveCultureResultsCommandHandler(db).Handle(
        new SaveCultureResultsCommand(10,null,null,null,null,null,null,[new(1,(int)SensitivityCategory.ModerateFor)]),default);

    Assert.True(r.IsSuccess);
    Assert.Equal(SensitivityCategory.ModerateFor,Assert.Single(db.CultureAntibioticResults).SensitivityCategory);
}

[Fact]public async Task SaveCulture_PersistsLowSensitivityAsLowFor()
{
    var db=Seed();
    var r=await new SaveCultureResultsCommandHandler(db).Handle(
        new SaveCultureResultsCommand(10,null,null,null,null,null,null,[new(1,(int)SensitivityCategory.LowFor)]),default);

    Assert.True(r.IsSuccess);
    Assert.Equal(SensitivityCategory.LowFor,Assert.Single(db.CultureAntibioticResults).SensitivityCategory);
}

[Fact]public async Task SaveCulture_PersistsResistantAsResistantFor()
{
    var db=Seed();
    var r=await new SaveCultureResultsCommandHandler(db).Handle(
        new SaveCultureResultsCommand(10,null,null,null,null,null,null,[new(1,(int)SensitivityCategory.ResistantFor)]),default);

    Assert.True(r.IsSuccess);
    Assert.Equal(SensitivityCategory.ResistantFor,Assert.Single(db.CultureAntibioticResults).SensitivityCategory);
}

/// <summary>The original defect: a null row alongside a non-null one used to delete itself from the store.</summary>
[Fact]public async Task SaveCulture_NullAndNonNullRows_Coexist()
{
    var db=Seed();
    db.CultureAntibioticAttachments.Add(new CultureAntibioticAttachment(TestId.Create(1),AntibioticId.Create(2)));
    var save=new SaveCultureResultsCommandHandler(db);

    Assert.True((await save.Handle(new SaveCultureResultsCommand(10,null,null,null,null,null,null,
        [new(1,null),new(2,(int)SensitivityCategory.ResistantFor)]),default)).IsSuccess);

    Assert.Equal(2,db.CultureAntibioticResults.Count);
    Assert.Null(db.CultureAntibioticResults.Single(x=>x.AntibioticId.Value==1).SensitivityCategory);
    Assert.Equal(SensitivityCategory.ResistantFor,db.CultureAntibioticResults.Single(x=>x.AntibioticId.Value==2).SensitivityCategory);
}

private static FakeApplicationDbContext Seed(){var db=new FakeApplicationDbContext();db.Patients.Add(Patient.Create(PatientId.Create(1),"P",Sex.Female,25,AgeUnit.Year,DateTime.UtcNow));db.Tests.Add(Test.Create(TestId.Create(1),"C","C","C","C",1,100m,ResultKind.Culture,true));db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(10),PatientId.Create(1),TestId.Create(1),100));db.CultureAntibioticAttachments.Add(new CultureAntibioticAttachment(TestId.Create(1),AntibioticId.Create(1)));return db;}}
