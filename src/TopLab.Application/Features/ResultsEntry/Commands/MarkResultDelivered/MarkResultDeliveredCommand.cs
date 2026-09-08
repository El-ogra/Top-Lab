using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;

namespace TopLab.Application.Features.ResultsEntry.Commands.MarkResultDelivered;

public sealed record MarkResultDeliveredCommand(int PatientTestId) : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ResultsEntryAccessPolicy.DeliverResults;
}
