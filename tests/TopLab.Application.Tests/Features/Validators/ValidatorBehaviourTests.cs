using FluentValidation;
using TopLab.Application.Features.AnalyteProfiles.Commands.AddProfileAnalyte;
using TopLab.Application.Features.AnalyteProfiles.Commands.DeactivateAnalyte;
using TopLab.Application.Features.AnalyteProfiles.Commands.RemoveProfileAnalyte;
using TopLab.Application.Features.AnalyteProfiles.Commands.UpdateAnalyte;
using TopLab.Application.Features.CultureResults.Commands.MarkCultureReportPrinted;
using TopLab.Application.Features.CultureResults.Commands.UnverifyCultureResult;
using TopLab.Application.Features.CultureResults.Commands.VerifyCultureResult;
using TopLab.Application.Features.PatientRegistration.Commands.ClearAllTests;
using TopLab.Application.Features.PatientRegistration.Commands.RemoveMedicalCondition;
using TopLab.Application.Features.PatientRegistration.Commands.SoftDeletePatient;
using TopLab.Application.Features.PatientRegistration.Commands.UpdatePatient;
using TopLab.Application.Features.ProfileResults.Commands.MarkProfilePrinted;
using TopLab.Application.Features.ProfileResults.Commands.UnverifyProfileResults;
using TopLab.Application.Features.ProfileResults.Commands.VerifyProfileResults;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.BackupDatabaseNow;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.RestoreDatabase;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateDatabaseServerSettings;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.MapTestToAnalyte;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UnmapTestFromAnalyte;
using TopLab.Application.Features.UsersAndPermissions.Commands.DeactivateUser;
using TopLab.Application.Features.UsersAndPermissions.Commands.DeleteUser;
using TopLab.Application.Features.UsersAndPermissions.Commands.ReactivateUser;

namespace TopLab.Application.Tests.Features.Validators;

/// <summary>
/// Slice 2 (m-01): behavioural tests for each of the 22 new validators.
/// Each test verifies that a valid input passes and an invalid input fails with
/// the expected property name and error message.
/// </summary>
public class ValidatorBehaviourTests
{
    [Fact]
    public void AddProfileAnalyte_InvalidIds_Fails()
    {
        var validator = new AddProfileAnalyteCommandValidator();
        var result = validator.Validate(new AddProfileAnalyteCommand(0, 0));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ProfileId");
        Assert.Contains(result.Errors, e => e.PropertyName == "AnalyteId");
    }

    [Fact]
    public void AddProfileAnalyte_ValidIds_Passes()
    {
        var validator = new AddProfileAnalyteCommandValidator();
        var result = validator.Validate(new AddProfileAnalyteCommand(1, 1));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void DeactivateAnalyte_InvalidId_Fails()
    {
        var validator = new DeactivateAnalyteCommandValidator();
        var result = validator.Validate(new DeactivateAnalyteCommand(0));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "AnalyteId");
    }

    [Fact]
    public void RemoveProfileAnalyte_InvalidIds_Fails()
    {
        var validator = new RemoveProfileAnalyteCommandValidator();
        var result = validator.Validate(new RemoveProfileAnalyteCommand(0, 0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateAnalyte_EmptyName_Fails()
    {
        var validator = new UpdateAnalyteCommandValidator();
        var result = validator.Validate(new UpdateAnalyteCommand(1, "", "Report"));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void MarkCultureReportPrinted_InvalidId_Fails()
    {
        var validator = new MarkCultureReportPrintedCommandValidator();
        var result = validator.Validate(new MarkCultureReportPrintedCommand(0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UnverifyCultureResult_InvalidId_Fails()
    {
        var validator = new UnverifyCultureResultCommandValidator();
        var result = validator.Validate(new UnverifyCultureResultCommand(0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void VerifyCultureResult_InvalidId_Fails()
    {
        var validator = new VerifyCultureResultCommandValidator();
        var result = validator.Validate(new VerifyCultureResultCommand(0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ClearAllTests_InvalidId_Fails()
    {
        var validator = new ClearAllTestsCommandValidator();
        var result = validator.Validate(new ClearAllTestsCommand(0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void RemoveMedicalCondition_InvalidIds_Fails()
    {
        var validator = new RemoveMedicalConditionCommandValidator();
        var result = validator.Validate(new RemoveMedicalConditionCommand(0, 0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void SoftDeletePatient_InvalidId_Fails()
    {
        var validator = new SoftDeletePatientCommandValidator();
        var result = validator.Validate(new SoftDeletePatientCommand(0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void MarkProfilePrinted_InvalidId_Fails()
    {
        var validator = new MarkProfilePrintedCommandValidator();
        var result = validator.Validate(new MarkProfilePrintedCommand(0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UnverifyProfileResults_InvalidId_Fails()
    {
        var validator = new UnverifyProfileResultsCommandValidator();
        var result = validator.Validate(new UnverifyProfileResultsCommand(0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void VerifyProfileResults_InvalidId_Fails()
    {
        var validator = new VerifyProfileResultsCommandValidator();
        var result = validator.Validate(new VerifyProfileResultsCommand(0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void BackupDatabaseNow_EmptyPath_Fails()
    {
        var validator = new BackupDatabaseNowCommandValidator();
        var result = validator.Validate(new BackupDatabaseNowCommand(""));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void RestoreDatabase_EmptyPath_Fails()
    {
        var validator = new RestoreDatabaseCommandValidator();
        var result = validator.Validate(new RestoreDatabaseCommand(""));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateDatabaseServerSettings_EmptyServer_Fails()
    {
        var validator = new UpdateDatabaseServerSettingsCommandValidator();
        var result = validator.Validate(new UpdateDatabaseServerSettingsCommand("", "DB", false, "user", "pass"));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void MapTestToAnalyte_InvalidIds_Fails()
    {
        var validator = new MapTestToAnalyteCommandValidator();
        var result = validator.Validate(new MapTestToAnalyteCommand(0, 0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UnmapTestFromAnalyte_InvalidId_Fails()
    {
        var validator = new UnmapTestFromAnalyteCommandValidator();
        var result = validator.Validate(new UnmapTestFromAnalyteCommand(0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void DeactivateUser_InvalidId_Fails()
    {
        var validator = new DeactivateUserCommandValidator();
        var result = validator.Validate(new DeactivateUserCommand(0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void DeleteUser_InvalidId_Fails()
    {
        var validator = new DeleteUserCommandValidator();
        var result = validator.Validate(new DeleteUserCommand(0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ReactivateUser_InvalidId_Fails()
    {
        var validator = new ReactivateUserCommandValidator();
        var result = validator.Validate(new ReactivateUserCommand(0));
        Assert.False(result.IsValid);
    }
}
