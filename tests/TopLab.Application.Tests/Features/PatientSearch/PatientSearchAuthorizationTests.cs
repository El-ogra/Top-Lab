using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Features.PatientSearch.Queries.GetPatientByLabId;
using TopLab.Application.Features.PatientSearch.Queries.GetVisitDetail;
using TopLab.Application.Features.PatientSearch.Queries.GetVisitHistory;
using TopLab.Application.Features.PatientSearch.Queries.SearchPatientsGlobal;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientSearch;

/// <summary>
/// M-08 authorization shape: all four read queries are plain <see cref="IBaseRequest"/>
/// requests carrying no <see cref="IAuthorizedRequest"/> surface. The PatientSearch
/// feature is permission-light by design (FR-M08 FRs read only); it must not start
/// advertising a permission code the Presentation layer would then be forced to gate.
/// </summary>
public class PatientSearchAuthorizationTests
{
    [Theory]
    [InlineData(typeof(SearchPatientsGlobalQuery))]
    [InlineData(typeof(GetPatientByLabIdQuery))]
    [InlineData(typeof(GetVisitHistoryQuery))]
    [InlineData(typeof(GetVisitDetailQuery))]
    public void ReadQuery_IsPlainMediatRRequest_WithoutAuthorizationSurface(System.Type queryType)
    {
        Assert.False(typeof(IAuthorizedRequest).IsAssignableFrom(queryType));
        Assert.True(typeof(IBaseRequest).IsAssignableFrom(queryType));
    }
}