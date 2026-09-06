using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UpdateTest;

public sealed record UpdateTestCommand(
    int Id,
    string Name,
    string ReportName,
    string ReceiptName,
    string TestCode,
    int CompletionDurationMinutes,
    decimal PatientPrice,
    int? TestGroupId,
    string? Barcode,
    bool IsSentOut,
    decimal? SentOutCostPrice,
    decimal? LabToLabPrice)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}