using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Statistics.Common;
using TopLab.Application.Features.Statistics.Queries.GetPatientCountStatistics;
using TopLab.Application.Features.Statistics.Queries.GetSentOutStatistics;
using TopLab.Application.Features.Statistics.Queries.GetTestCountStatistics;
using TopLab.Application.Features.Statistics.Queries.GetUserProductivityStatistics;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.Statistics;

public class StatisticsAuthorizationTests
{
    private const string ExpectedDenial = "أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام";

    public static TheoryData<IAuthorizedRequest> ModuleQueries => new()
    {
        { new GetPatientCountStatisticsQuery(null, null, true, true, true, false) },
        { new GetTestCountStatisticsQuery(null, null, null) },
        { new GetSentOutStatisticsQuery(null, null, null) },
        { new GetUserProductivityStatisticsQuery(null, null, null) },
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

    [Fact]
    public async Task SentOutStatistics_DeniedWithoutPermission_ReturnsStandardMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<GetSentOutStatisticsQuery, Result<SentOutStatisticsDto>>(user);

        var response = await behavior.Handle(
            new GetSentOutStatisticsQuery(null, null, null),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ExpectedDenial, response.Error!.Message);
    }

    [Fact]
    public async Task SentOutStatistics_AbsolutePermission_BypassesWithoutGrant()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = true };
        var behavior = new AuthorizationBehavior<GetSentOutStatisticsQuery, Result<SentOutStatisticsDto>>(user);

        var response = await behavior.Handle(
            new GetSentOutStatisticsQuery(null, null, null),
            _ => Task.FromResult(Result<SentOutStatisticsDto>.Success(null!)),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task UserProductivityStatistics_DeniedWithoutPermission_ReturnsStandardMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<GetUserProductivityStatisticsQuery, Result<UserProductivityStatisticsDto>>(user);

        var response = await behavior.Handle(
            new GetUserProductivityStatisticsQuery(null, null, null),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ExpectedDenial, response.Error!.Message);
    }

    [Fact]
    public async Task UserProductivityStatistics_AbsolutePermission_BypassesWithoutGrant()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = true };
        var behavior = new AuthorizationBehavior<GetUserProductivityStatisticsQuery, Result<UserProductivityStatisticsDto>>(user);

        var response = await behavior.Handle(
            new GetUserProductivityStatisticsQuery(null, null, null),
            _ => Task.FromResult(Result<UserProductivityStatisticsDto>.Success(null!)),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
    }
}
