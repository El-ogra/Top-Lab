using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;

namespace TopLab.Application.Features.PatientEnvelope.Commands.PrintEnvelope;

/// <summary>
/// Prints the patient envelope for a visit from the results screen
/// (<c>طباعة ظرف</c>). Gated on <c>PRINT_RESULTS</c>, consistent with the
/// sibling results-screen print actions (BulkPrint / Export / MarkPrinted).
/// </summary>
public sealed record PrintEnvelopeCommand(int PatientId)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ResultsEntryAccessPolicy.PrintResults;
}
