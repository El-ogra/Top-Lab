using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateTest;

public sealed record CreateTestCommand(
    string Name,
    string ReportName,
    string ReceiptName,
    string TestCode,
    int CompletionDurationMinutes,
    decimal PatientPrice,
    ResultKind ResultKind,
    bool IsCultureType,
    int? TestGroupId,
    string? Barcode,
    bool IsSentOut,
    decimal? SentOutCostPrice,
    decimal? LabToLabPrice)
    : IRequest<Result<int>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}