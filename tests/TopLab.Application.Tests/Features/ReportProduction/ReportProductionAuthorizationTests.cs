using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Commands.PrintBlankReport;
using TopLab.Application.Features.ReportProduction.Commands.PrintCombinedReport;
using TopLab.Application.Features.ReportProduction.Commands.PrintHistoryReport;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.ReportProduction;

public class ReportProductionAuthorizationTests
{
    [Theory]
    [InlineData(typeof(PrintCombinedReportCommand))]
    [InlineData(typeof(PrintBlankReportCommand))]
    [InlineData(typeof(PrintHistoryReportCommand))]
    public void PrintGate_Requires_PrintResults(System.Type commandType)
    {
        IAuthorizedRequest authorized = commandType.Name switch
        {
            nameof(PrintCombinedReportCommand) => new PrintCombinedReportCommand(1, new[] { 101 }),
            nameof(PrintBlankReportCommand) => new PrintBlankReportCommand(1),
            nameof(PrintHistoryReportCommand) => new PrintHistoryReportCommand(1),
            _ => throw new InvalidOperationException()
        };

        Assert.Equal("PRINT_RESULTS", authorized.RequiredPermissionCode);
    }

    [Theory]
    [InlineData(typeof(PrintCombinedReportCommand))]
    [InlineData(typeof(PrintBlankReportCommand))]
    [InlineData(typeof(PrintHistoryReportCommand))]
    public void AccessPolicy_Constant_Mirrors_Gate(System.Type commandType)
    {
        IAuthorizedRequest authorized = commandType.Name switch
        {
            nameof(PrintCombinedReportCommand) => new PrintCombinedReportCommand(1, new[] { 101 }),
            nameof(PrintBlankReportCommand) => new PrintBlankReportCommand(1),
            nameof(PrintHistoryReportCommand) => new PrintHistoryReportCommand(1),
            _ => throw new InvalidOperationException()
        };

        Assert.Equal("PRINT_RESULTS", ReportProductionAccessPolicy.PrintResults);
        Assert.Equal(ReportProductionAccessPolicy.PrintResults, authorized.RequiredPermissionCode);
    }

    [Fact]
    public async Task WithoutPermission_ReturnsForbidden_HandlerNeverRuns()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false, GrantedPermissions = { } };
        var behavior = new AuthorizationBehavior<PrintCombinedReportCommand, Result>(user);

        var response = await behavior.Handle(
            new PrintCombinedReportCommand(1, new[] { 101 }),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, response.Error!.Type);
        Assert.Equal("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام", response.Error.Message);
    }

    [Fact]
    public async Task WithPermission_InvokesNextHandler()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false, GrantedPermissions = { ReportProductionAccessPolicy.PrintResults } };
        var behavior = new AuthorizationBehavior<PrintBlankReportCommand, Result>(user);

        var response = await behavior.Handle(
            new PrintBlankReportCommand(1),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
    }
}