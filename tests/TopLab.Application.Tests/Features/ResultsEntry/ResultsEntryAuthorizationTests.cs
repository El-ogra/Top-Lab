using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Commands.BulkPrint;
using TopLab.Application.Features.ResultsEntry.Commands.ClearResult;
using TopLab.Application.Features.ResultsEntry.Commands.EnterResult;
using TopLab.Application.Features.ResultsEntry.Commands.ExportPatientReportPdf;
using TopLab.Application.Features.ResultsEntry.Commands.MarkAllPatientResultsReviewed;
using TopLab.Application.Features.ResultsEntry.Commands.MarkResultDelivered;
using TopLab.Application.Features.ResultsEntry.Commands.MarkResultPrinted;
using TopLab.Application.Features.ResultsEntry.Commands.RefreshResultReferenceRange;
using TopLab.Application.Features.ResultsEntry.Commands.ReviewResult;
using TopLab.Application.Features.ResultsEntry.Commands.UnreviewResult;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultsEntry;

public class ResultsEntryAuthorizationTests
{
    [Theory]
    [InlineData(typeof(EnterResultCommand))]
    [InlineData(typeof(ClearResultCommand))]
    [InlineData(typeof(RefreshResultReferenceRangeCommand))]
    public void EditGate_Requires_EditResults(System.Type commandType)
    {
        IAuthorizedRequest authorized = commandType.Name switch
        {
            nameof(EnterResultCommand) => new EnterResultCommand(1, "5"),
            nameof(ClearResultCommand) => new ClearResultCommand(1),
            nameof(RefreshResultReferenceRangeCommand) => new RefreshResultReferenceRangeCommand(1),
            _ => throw new InvalidOperationException()
        };

        Assert.Equal("EDIT_RESULTS", authorized.RequiredPermissionCode);
    }

    [Theory]
    [InlineData(typeof(ReviewResultCommand))]
    [InlineData(typeof(UnreviewResultCommand))]
    [InlineData(typeof(MarkAllPatientResultsReviewedCommand))]
    public void ReviewGate_Requires_ReviewResults(System.Type commandType)
    {
        IAuthorizedRequest authorized = commandType.Name switch
        {
            nameof(ReviewResultCommand) => new ReviewResultCommand(1),
            nameof(UnreviewResultCommand) => new UnreviewResultCommand(1),
            nameof(MarkAllPatientResultsReviewedCommand) => new MarkAllPatientResultsReviewedCommand(1),
            _ => throw new InvalidOperationException()
        };

        Assert.Equal("REVIEW_RESULTS", authorized.RequiredPermissionCode);
    }

    [Theory]
    [InlineData(typeof(MarkResultPrintedCommand))]
    [InlineData(typeof(ExportPatientReportPdfCommand))]
    [InlineData(typeof(ExecuteBulkPrintCommand))]
    public void PrintGate_Requires_PrintResults(System.Type commandType)
    {
        IAuthorizedRequest authorized = commandType.Name switch
        {
            nameof(MarkResultPrintedCommand) => new MarkResultPrintedCommand(1),
            nameof(ExportPatientReportPdfCommand) => new ExportPatientReportPdfCommand(1, "C:\\x.pdf"),
            nameof(ExecuteBulkPrintCommand) => new ExecuteBulkPrintCommand(new[] { new BulkPrintDecision(1, true) }),
            _ => throw new InvalidOperationException()
        };

        Assert.Equal("PRINT_RESULTS", authorized.RequiredPermissionCode);
    }

    [Fact]
    public void DeliverGate_Requires_DeliverResults()
    {
        IAuthorizedRequest authorized = new MarkResultDeliveredCommand(1);
        Assert.Equal("DELIVER_RESULTS", authorized.RequiredPermissionCode);
    }

    [Fact]
    public void AccessPolicy_Constants_Match_PermissionCatalog()
    {
        Assert.Equal("EDIT_RESULTS", ResultsEntryAccessPolicy.EditResults);
        Assert.Equal("REVIEW_RESULTS", ResultsEntryAccessPolicy.ReviewResults);
        Assert.Equal("PRINT_RESULTS", ResultsEntryAccessPolicy.PrintResults);
        Assert.Equal("DELIVER_RESULTS", ResultsEntryAccessPolicy.DeliverResults);
    }

    [Fact]
    public async Task WithoutPermission_ReturnsForbidden_WithSharedMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false, GrantedPermissions = { } };
        var behavior = new AuthorizationBehavior<EnterResultCommand, Result>(user);

        var response = await behavior.Handle(
            new EnterResultCommand(1, "5"),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, response.Error!.Type);
        Assert.Equal("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام", response.Error.Message);
    }
}
