using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetByTestGroup;
using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetByWorkGroupLog;
using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetSummary;
using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetTestCountByPeriod;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.WorkSheets;

public class WorkSheetsAuthorizationTests
{
    private const string ExpectedDenial = "أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام";

    [Fact]
    public void Queries_DeclarePrintWorksheetPermissionCode()
    {
        Assert.Equal("PRINT_WORKSHEET", new GetWorkSheetByWorkGroupLogQuery(1).RequiredPermissionCode);
        Assert.Equal("PRINT_WORKSHEET", new GetWorkSheetByTestGroupQuery().RequiredPermissionCode);
        Assert.Equal("PRINT_WORKSHEET", new GetWorkSheetSummaryQuery().RequiredPermissionCode);
        Assert.Equal("PRINT_WORKSHEET", new GetWorkSheetTestCountByPeriodQuery().RequiredPermissionCode);
    }

    [Fact]
    public async Task ByLog_DeniedWithoutPermission_ReturnsStandardMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<GetWorkSheetByWorkGroupLogQuery, Result<WorkSheetDto>>(user);

        var response = await behavior.Handle(
            new GetWorkSheetByWorkGroupLogQuery(1),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ExpectedDenial, response.Error!.Message);
    }

    [Fact]
    public async Task ByTestGroup_DeniedWithoutPermission_ReturnsStandardMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<GetWorkSheetByTestGroupQuery, Result<WorkSheetDto>>(user);

        var response = await behavior.Handle(
            new GetWorkSheetByTestGroupQuery(),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ExpectedDenial, response.Error!.Message);
    }

    [Fact]
    public async Task Summary_DeniedWithoutPermission_ReturnsStandardMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<GetWorkSheetSummaryQuery, Result<IReadOnlyList<WorkSheetSummaryRowDto>>>(user);

        var response = await behavior.Handle(
            new GetWorkSheetSummaryQuery(),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ExpectedDenial, response.Error!.Message);
    }

    [Fact]
    public async Task TestCount_DeniedWithoutPermission_ReturnsStandardMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<GetWorkSheetTestCountByPeriodQuery, Result<WorkSheetTestCountDto>>(user);

        var response = await behavior.Handle(
            new GetWorkSheetTestCountByPeriodQuery(),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ExpectedDenial, response.Error!.Message);
    }
}
