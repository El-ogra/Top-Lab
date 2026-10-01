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

private static FakeApplicationDbContext Seed(){var db=new FakeApplicationDbContext();db.Patients.Add(Patient.Create(PatientId.Create(1),"P",Sex.Female,25,AgeUnit.Year,DateTime.UtcNow));db.Tests.Add(Test.Create(TestId.Create(1),"C","C","C","C",1,100m,ResultKind.Culture,true));db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(10),PatientId.Create(1),TestId.Create(1),100));db.CultureAntibioticAttachments.Add(new CultureAntibioticAttachment(TestId.Create(1),AntibioticId.Create(1)));return db;}

// ---- W-02 Slice 2 / SD-13: update-in-place instead of delete-all-then-recreate ----

/// <summary>The key guarantee: a row that already exists keeps its identity across a re-save.
/// From S9 this is what stops the first culture save from erasing every InhibitionZoneMm.</summary>
[Fact]public async Task SaveCulture_ExistingRow_KeepsItsId()
{
    var db=Seed();
    db.CultureAntibioticResults.Add(CultureAntibioticResult.Create(
        CultureAntibioticResultId.Create(777),PatientTestId.Create(10),AntibioticId.Create(1),SensitivityCategory.HighlyFor));

    var r=await new SaveCultureResultsCommandHandler(db).Handle(
        new SaveCultureResultsCommand(10,null,null,null,null,null,null,[new(1,(int)SensitivityCategory.ResistantFor)]),default);

    Assert.True(r.IsSuccess);
    var row=Assert.Single(db.CultureAntibioticResults);
    Assert.Equal(777,row.Id.Value);
    Assert.Equal(SensitivityCategory.ResistantFor,row.SensitivityCategory);
}

[Fact]public async Task SaveCulture_RemoveOneRow_KeepsTheOthers()
{
    var db=Seed();
    db.CultureAntibioticAttachments.Add(new CultureAntibioticAttachment(TestId.Create(1),AntibioticId.Create(2)));
    db.CultureAntibioticResults.Add(CultureAntibioticResult.Create(CultureAntibioticResultId.Create(701),PatientTestId.Create(10),AntibioticId.Create(1),SensitivityCategory.HighlyFor));
    db.CultureAntibioticResults.Add(CultureAntibioticResult.Create(CultureAntibioticResultId.Create(702),PatientTestId.Create(10),AntibioticId.Create(2),SensitivityCategory.LowFor));

    var r=await new SaveCultureResultsCommandHandler(db).Handle(
        new SaveCultureResultsCommand(10,null,null,null,null,null,null,[new(1,(int)SensitivityCategory.HighlyFor)]),default);

    Assert.True(r.IsSuccess);
    var row=Assert.Single(db.CultureAntibioticResults);
    Assert.Equal(701,row.Id.Value);
    Assert.Equal(1,row.AntibioticId.Value);
}

[Fact]public async Task SaveCulture_AddNewRow_WhileKeepingExisting()
{
    var db=Seed();
    db.CultureAntibioticAttachments.Add(new CultureAntibioticAttachment(TestId.Create(1),AntibioticId.Create(2)));
    db.CultureAntibioticResults.Add(CultureAntibioticResult.Create(CultureAntibioticResultId.Create(701),PatientTestId.Create(10),AntibioticId.Create(1),SensitivityCategory.HighlyFor));

    var r=await new SaveCultureResultsCommandHandler(db).Handle(
        new SaveCultureResultsCommand(10,null,null,null,null,null,null,
            [new(1,(int)SensitivityCategory.HighlyFor),new(2,(int)SensitivityCategory.ResistantFor)]),default);

    Assert.True(r.IsSuccess);
    Assert.Equal(2,db.CultureAntibioticResults.Count);
    Assert.Equal(701,db.CultureAntibioticResults.Single(x=>x.AntibioticId.Value==1).Id.Value);
    Assert.Equal(SensitivityCategory.ResistantFor,db.CultureAntibioticResults.Single(x=>x.AntibioticId.Value==2).SensitivityCategory);
}

[Fact]public async Task SaveCulture_TwiceInARow_IsIdempotent()
{
    var db=Seed();
    var save=new SaveCultureResultsCommandHandler(db);
    var cmd=new SaveCultureResultsCommand(10," sample ","E.coli",null,null,null,null,[new(1,(int)SensitivityCategory.ModerateFor)]);

    Assert.True((await save.Handle(cmd,default)).IsSuccess);
    var first=Assert.Single(db.CultureAntibioticResults);
    Assert.True((await save.Handle(cmd,default)).IsSuccess);

    Assert.Single(db.CultureAntibioticResults);
    Assert.Equal(SensitivityCategory.ModerateFor,first.SensitivityCategory);
    // CultureResult trims on construction — pre-existing behaviour, asserted here as observed.
    Assert.Equal("sample",Assert.Single(db.CultureResults).Sample);
}

[Fact]public async Task SaveCulture_HeaderUpdate_UnaffectedByRowDiffing()
{
    var db=Seed();
    var save=new SaveCultureResultsCommandHandler(db);

    Assert.True((await save.Handle(new SaveCultureResultsCommand(10," first ","E.coli",null,null," aerobic ",null,
        [new(1,(int)SensitivityCategory.HighlyFor)]),default)).IsSuccess);
    Assert.True((await save.Handle(new SaveCultureResultsCommand(10," second ",null,null,null," anaerobic ",null,
        [new(1,(int)SensitivityCategory.HighlyFor)]),default)).IsSuccess);

    var header=Assert.Single(db.CultureResults);
    Assert.Equal("second",header.Sample);
    Assert.Equal("anaerobic",header.CultureCondition);
    Assert.Single(db.CultureAntibioticResults);
}

}
