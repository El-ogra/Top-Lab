using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.AddCustomGroupToVisit;
using TopLab.Application.Features.PatientRegistration.Commands.AddMedicalCondition;
using TopLab.Application.Features.PatientRegistration.Commands.AddTestsToVisit;
using TopLab.Application.Features.PatientRegistration.Commands.ClearAllTests;
using TopLab.Application.Features.PatientRegistration.Commands.CreatePatient;
using TopLab.Application.Features.PatientRegistration.Commands.RemoveMedicalCondition;
using TopLab.Application.Features.PatientRegistration.Commands.RemoveTestFromVisit;
using TopLab.Application.Features.PatientRegistration.Commands.SoftDeletePatient;
using TopLab.Application.Features.PatientRegistration.Commands.UpdatePatient;
using TopLab.Application.Features.PatientRegistration.Commands.UpdatePatientTestSampleFlags;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Commands;

public class PatientRegistrationAuthorizationTests
{
    private static object CreateInstance(System.Type commandType)
    {
        if (commandType == typeof(CreatePatientCommand))
        {
            return new CreatePatientCommand(
                "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow, AccountType.Individual,
                false, null, null, null, null, null, null, null, false, null, false, null,
                Array.Empty<PatientNumberInput>(), Array.Empty<int>(),
                new[] { new AddTestInput(10, false, false, true, false, false, false) });
        }
        if (commandType == typeof(UpdatePatientCommand))
        {
            return new UpdatePatientCommand(
                1, "X", Sex.Male, 30, AgeUnit.Year, null, null, null, false, AccountType.Individual,
                null, false, null, false, Array.Empty<PatientNumberInput>());
        }
        if (commandType == typeof(SoftDeletePatientCommand))
        {
            return new SoftDeletePatientCommand(1);
        }
        if (commandType == typeof(AddMedicalConditionCommand))
        {
            return new AddMedicalConditionCommand(1, 1);
        }
        if (commandType == typeof(RemoveMedicalConditionCommand))
        {
            return new RemoveMedicalConditionCommand(1, 1);
        }
        if (commandType == typeof(AddTestsToVisitCommand))
        {
            return new AddTestsToVisitCommand(1, new[]
            {
                new AddTestInput(10, false, false, true, false, false, false)
            });
        }
        if (commandType == typeof(AddCustomGroupToVisitCommand))
        {
            return new AddCustomGroupToVisitCommand(1, 1);
        }
        if (commandType == typeof(RemoveTestFromVisitCommand))
        {
            return new RemoveTestFromVisitCommand(1);
        }
        if (commandType == typeof(UpdatePatientTestSampleFlagsCommand))
        {
            return new UpdatePatientTestSampleFlagsCommand(1, true, false, false, false, false, false);
        }
        if (commandType == typeof(ClearAllTestsCommand))
        {
            return new ClearAllTestsCommand(1);
        }
        throw new InvalidOperationException($"Unhandled command type {commandType.Name}.");
    }

    [Theory]
    [InlineData(typeof(CreatePatientCommand))]
    [InlineData(typeof(UpdatePatientCommand))]
    [InlineData(typeof(AddMedicalConditionCommand))]
    [InlineData(typeof(RemoveMedicalConditionCommand))]
    [InlineData(typeof(AddTestsToVisitCommand))]
    [InlineData(typeof(AddCustomGroupToVisitCommand))]
    [InlineData(typeof(RemoveTestFromVisitCommand))]
    [InlineData(typeof(UpdatePatientTestSampleFlagsCommand))]
    [InlineData(typeof(ClearAllTestsCommand))]
    public void EveryWriteCommand_Requires_AddEditPatient(System.Type commandType)
    {
        var authorized = (IAuthorizedRequest)CreateInstance(commandType);
        Assert.Equal("ADD_EDIT_PATIENT", authorized.RequiredPermissionCode);
    }

    [Fact]
    public void SoftDelete_Requires_DeletePatient()
    {
        var authorized = (IAuthorizedRequest)CreateInstance(typeof(SoftDeletePatientCommand));
        Assert.Equal("DELETE_PATIENT", authorized.RequiredPermissionCode);
    }

    [Fact]
    public async Task WithoutPermission_ReturnsForbidden_WithSharedMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false, GrantedPermissions = { } };
        var behavior = new AuthorizationBehavior<CreatePatientCommand, Result<int>>(user);

        var response = await behavior.Handle(
            (CreatePatientCommand)CreateInstance(typeof(CreatePatientCommand)),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, response.Error!.Type);
        Assert.Equal("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام", response.Error.Message);
    }

    [Fact]
    public async Task WithoutDeletePatientPermission_ReturnsForbidden()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false, GrantedPermissions = { "ADD_EDIT_PATIENT" } };
        var behavior = new AuthorizationBehavior<SoftDeletePatientCommand, Result<bool>>(user);

        var response = await behavior.Handle(
            new SoftDeletePatientCommand(1),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, response.Error!.Type);
    }
}