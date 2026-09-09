using TopLab.Application.Common.Authorization;
using TopLab.Application.Features.ProfileResults.Commands.AmendProfileResult;
using TopLab.Application.Features.ProfileResults.Commands.MarkProfilePrinted;
using TopLab.Application.Features.ProfileResults.Commands.SaveProfileResults;
using TopLab.Application.Features.ProfileResults.Commands.UnverifyProfileResults;
using TopLab.Application.Features.ProfileResults.Commands.VerifyProfileResults;
using TopLab.Application.Features.ProfileResults.Queries.GetProfileEntryGrid;
using TopLab.Application.Features.ProfileResults.Queries.GetProfileReport;
using TopLab.Application.Features.ProfileResults.Queries.GetProfileResultAmendments;
using Xunit;

namespace TopLab.Application.Tests.Features.ProfileResults;

public class ProfileResultsAuthorizationTests
{
    [Fact]
    public void SaveProfileResults_Requires_EditResults()
    {
        Assert.Equal("EDIT_RESULTS", ((IAuthorizedRequest)new SaveProfileResultsCommand(1, null, Array.Empty<ProfileItemInput>())).RequiredPermissionCode);
    }

    [Fact]
    public void VerifyProfileResults_Requires_ReviewResults()
    {
        Assert.Equal("REVIEW_RESULTS", ((IAuthorizedRequest)new VerifyProfileResultsCommand(1)).RequiredPermissionCode);
    }

    [Fact]
    public void UnverifyProfileResults_Requires_ReviewResults()
    {
        Assert.Equal("REVIEW_RESULTS", ((IAuthorizedRequest)new UnverifyProfileResultsCommand(1)).RequiredPermissionCode);
    }

    [Fact]
    public void MarkProfilePrinted_Requires_PrintResults()
    {
        Assert.Equal("PRINT_RESULTS", ((IAuthorizedRequest)new MarkProfilePrintedCommand(1)).RequiredPermissionCode);
    }

    [Fact]
    public void AmendProfileResult_Requires_EditResults()
    {
        Assert.Equal("EDIT_RESULTS", ((IAuthorizedRequest)new AmendProfileResultCommand(1, "5", null, null, null)).RequiredPermissionCode);
    }

    [Fact]
    public void GetProfileEntryGrid_Requires_EditResults()
    {
        Assert.Equal("EDIT_RESULTS", ((IAuthorizedRequest)new GetProfileEntryGridQuery(1)).RequiredPermissionCode);
    }

    [Fact]
    public void GetProfileReport_Requires_EditResults()
    {
        Assert.Equal("EDIT_RESULTS", ((IAuthorizedRequest)new GetProfileReportQuery(1)).RequiredPermissionCode);
    }

    [Fact]
    public void GetProfileResultAmendments_Requires_AuditAccess()
    {
        Assert.Equal("PT_AUDIT_ACCESS", ((IAuthorizedRequest)new GetProfileResultAmendmentsQuery(1)).RequiredPermissionCode);
    }
}