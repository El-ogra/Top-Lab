using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.SystemAndPrintSettings.Queries.GetPrinterAssignments;

public sealed class GetPrinterAssignmentsQueryHandler : IRequestHandler<GetPrinterAssignmentsQuery, Result<IReadOnlyList<PrinterAssignmentDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetPrinterAssignmentsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<PrinterAssignmentDto>>> Handle(
        GetPrinterAssignmentsQuery request,
        CancellationToken cancellationToken)
    {
        var assignments = _db.Set<PrinterAssignment>()
            .OrderBy(p => p.OutputType)
            .Select(p => new PrinterAssignmentDto(p.OutputType, p.PrinterName))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<PrinterAssignmentDto>>.Success(assignments));
    }
}