using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Features.SystemAndPrintSettings.Queries.GetPrinterAssignments;

public sealed record GetPrinterAssignmentsQuery : IRequest<Result<IReadOnlyList<PrinterAssignmentDto>>>;