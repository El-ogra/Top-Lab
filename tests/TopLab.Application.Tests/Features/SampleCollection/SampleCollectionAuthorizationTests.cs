using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SampleCollection.Commands.MarkAllSamplesDrawnForPatient;
using TopLab.Application.Features.SampleCollection.Commands.MarkSampleDrawn;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.SampleCollection;

public class SampleCollectionAuthorizationTests
{
    [Theory]
    [InlineData(typeof(MarkSampleDrawnCommand))]
    [InlineData(typeof(MarkAllSamplesDrawnForPatientCommand))]
    public void EveryWriteCommand_Requires_AddEditPatient(System.Type commandType)
    {
        IAuthorizedRequest authorized = commandType == typeof(MarkSampleDrawnCommand)
            ? new MarkSampleDrawnCommand(1, DateTime.UtcNow)
            : new MarkAllSamplesDrawnForPatientCommand(1);

        Assert.Equal("ADD_EDIT_PATIENT", authorized.RequiredPermissionCode);
    }

    [Fact]
    public async Task WithoutPermission_ReturnsForbidden_WithSharedMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false, GrantedPermissions = { } };
        var behavior = new AuthorizationBehavior<MarkSampleDrawnCommand, Result<bool>>(user);

        var response = await behavior.Handle(
            new MarkSampleDrawnCommand(1, DateTime.UtcNow),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, response.Error!.Type);
        Assert.Equal("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام", response.Error.Message);
    }
}
