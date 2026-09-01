using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.SavePrinterAssignments;

public sealed class SavePrinterAssignmentsCommandHandler : IRequestHandler<SavePrinterAssignmentsCommand, Result>
{
    private static readonly PrinterOutputType[] OutputTypes =
    [
        PrinterOutputType.Reports,
        PrinterOutputType.Barcode,
        PrinterOutputType.Envelope,
        PrinterOutputType.Receipt
    ];

    private readonly IApplicationDbContext _db;

    public SavePrinterAssignmentsCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(SavePrinterAssignmentsCommand request, CancellationToken cancellationToken)
    {
        var existing = _db.Set<PrinterAssignment>()
            .Where(p => OutputTypes.Contains(p.OutputType))
            .ToList();

        foreach (var assignment in existing)
        {
            var dto = request.Assignments.FirstOrDefault(a => a.OutputType == assignment.OutputType);
            if (dto != null)
            {
                assignment.ChangePrinter(dto.PrinterName);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}