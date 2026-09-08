using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;

namespace TopLab.Application.Features.ResultsEntry.Commands.ExportPatientReportPdf;

public sealed record ExportPatientReportPdfCommand(int PatientId, string AbsolutePath)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ResultsEntryAccessPolicy.PrintResults;
}
