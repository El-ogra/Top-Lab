using MediatR;using TopLab.Application.Common.Authorization;using TopLab.Application.Common.Results;using TopLab.Application.Features.CultureResults.Common;
namespace TopLab.Application.Features.CultureResults.Commands.MarkCultureReportPrinted;
public sealed record MarkCultureReportPrintedCommand(int PatientTestId):IRequest<Result>,IAuthorizedRequest{public string RequiredPermissionCode=>CultureResultsAccessPolicy.PrintResults;}
