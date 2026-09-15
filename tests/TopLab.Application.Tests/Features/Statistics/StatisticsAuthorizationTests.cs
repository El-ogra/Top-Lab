using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Statistics.Common;
using TopLab.Application.Features.Statistics.Queries.GetPatientCountStatistics;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.Statistics;

public class StatisticsAuthorizationTests
{
    private const string ExpectedDenial = "أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام";

    // Theory shell completed in S2/S3: remaining module queries are added as they ship.
    public static TheoryData<IAuthorizedRequest> ModuleQueries => new()
    {
        { new GetPatientCountStatisticsQuery(null, null, true, true, true, false) },
    };

    [Theory]
    [MemberData(nameof(ModuleQueries))]
    public void Queries_DeclareStatistics(IAuthorizedRequest query)
    {
        Assert.Equal("STATISTICS", query.RequiredPermissionCode);
        Assert.Equal(StatisticsAccessPolicy.Statistics, query.RequiredPermissionCode);
    }

    [Fact]
    public async Task PatientCountStatistics_DeniedWithoutPermission_ReturnsStandardMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<GetPatientCountStatisticsQuery, Result<PatientCountStatisticsDto>>(user);

        var response = await behavior.Handle(
            new GetPatientCountStatisticsQuery(null, null, true, true, true, false),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ExpectedDenial, response.Error!.Message);
    }

    [Fact]
    public async Task PatientCountStatistics_AbsolutePermission_BypassesWithoutGrant()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = true };
        var behavior = new AuthorizationBehavior<GetPatientCountStatisticsQuery, Result<PatientCountStatisticsDto>>(user);

        var response = await behavior.Handle(
            new GetPatientCountStatisticsQuery(null, null, true, true, true, false),
            _ => Task.FromResult(Result<PatientCountStatisticsDto>.Success(null!)),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
    }
}
