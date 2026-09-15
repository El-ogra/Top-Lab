using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Statistics.Common;
using TopLab.Application.Features.Statistics.Queries.GetPatientCountStatistics;
using TopLab.Application.Features.Statistics.Queries.GetTestCountStatistics;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.Statistics;

public class StatisticsAuthorizationTests
{
    private const string ExpectedDenial = "أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام";

    // Theory completed in S3: remaining module queries are added as they ship.
    public static TheoryData<IAuthorizedRequest> ModuleQueries => new()
    {
        { new GetPatientCountStatisticsQuery(null, null, true, true, true, false) },
        { new GetTestCountStatisticsQuery(null, null, null) },
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

    [Fact]
    public async Task TestCountStatistics_DeniedWithoutPermission_ReturnsStandardMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<GetTestCountStatisticsQuery, Result<TestCountStatisticsDto>>(user);

        var response = await behavior.Handle(
            new GetTestCountStatisticsQuery(null, null, null),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ExpectedDenial, response.Error!.Message);
    }

    [Fact]
    public async Task TestCountStatistics_AbsolutePermission_BypassesWithoutGrant()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = true };
        var behavior = new AuthorizationBehavior<GetTestCountStatisticsQuery, Result<TestCountStatisticsDto>>(user);

        var response = await behavior.Handle(
            new GetTestCountStatisticsQuery(null, null, null),
            _ => Task.FromResult(Result<TestCountStatisticsDto>.Success(null!)),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
    }
}
