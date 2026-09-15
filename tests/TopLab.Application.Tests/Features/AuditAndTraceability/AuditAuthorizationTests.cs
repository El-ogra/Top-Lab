using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AuditAndTraceability.Common;
using TopLab.Application.Features.AuditAndTraceability.Queries.GetPatientAudit;
using TopLab.Application.Features.AuditAndTraceability.Queries.GetPatientTestAudit;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.AuditAndTraceability;

public class AuditAuthorizationTests
{
    private const string ExpectedDenial = "أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام";

    // Theory shell completed in S2: both module queries are listed (plan §5 S1).
    public static TheoryData<IAuthorizedRequest> ModuleQueries => new()
    {
        { new GetPatientAuditQuery(1) },
        { new GetPatientTestAuditQuery(1) },
    };

    [Theory]
    [MemberData(nameof(ModuleQueries))]
    public void Queries_DeclarePtAuditAccess(IAuthorizedRequest query)
    {
        Assert.Equal("PT_AUDIT_ACCESS", query.RequiredPermissionCode);
        Assert.Equal(AuditAccessPolicy.PtAuditAccess, query.RequiredPermissionCode);
    }

    [Fact]
    public async Task PatientAudit_DeniedWithoutPermission_ReturnsStandardMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<GetPatientAuditQuery, Result<PatientAuditDto>>(user);

        var response = await behavior.Handle(
            new GetPatientAuditQuery(1),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ExpectedDenial, response.Error!.Message);
    }

    [Fact]
    public async Task PatientAudit_AbsolutePermission_BypassesWithoutGrant()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = true };
        var behavior = new AuthorizationBehavior<GetPatientAuditQuery, Result<PatientAuditDto>>(user);

        var response = await behavior.Handle(
            new GetPatientAuditQuery(1),
            _ => Task.FromResult(Result<PatientAuditDto>.Success(null!)),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task PatientTestAudit_DeniedWithoutPermission_ReturnsStandardMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<GetPatientTestAuditQuery, Result<PatientTestAuditDto>>(user);

        var response = await behavior.Handle(
            new GetPatientTestAuditQuery(1),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ExpectedDenial, response.Error!.Message);
    }

    [Fact]
    public async Task PatientTestAudit_AbsolutePermission_BypassesWithoutGrant()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = true };
        var behavior = new AuthorizationBehavior<GetPatientTestAuditQuery, Result<PatientTestAuditDto>>(user);

        var response = await behavior.Handle(
            new GetPatientTestAuditQuery(1),
            _ => Task.FromResult(Result<PatientTestAuditDto>.Success(null!)),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
    }
}
