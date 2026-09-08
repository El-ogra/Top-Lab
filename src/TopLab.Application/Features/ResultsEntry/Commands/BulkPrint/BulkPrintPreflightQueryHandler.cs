using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.ResultsEntry.Commands.BulkPrint;

public sealed class BulkPrintPreflightQueryHandler
    : IRequestHandler<BulkPrintPreflightQuery, Result<IReadOnlyList<BulkPrintPreflightDto>>>
{
    private readonly IApplicationDbContext _db;

    public BulkPrintPreflightQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<BulkPrintPreflightDto>>> Handle(
        BulkPrintPreflightQuery request, CancellationToken cancellationToken)
    {
        var result = new List<BulkPrintPreflightDto>();

        foreach (var patientId in request.PatientIds.Distinct().ToList())
        {
            var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == patientId);
            if (patient is null || patient.IsDeleted)
            {
                continue;
            }

            var tests = _db.Set<PatientTest>()
                .Where(pt => pt.PatientId.Value == patient.Id.Value)
                .ToList();

            var verified = tests
                .Where(pt => pt.EnteredAtUtc is not null && pt.IsReviewed)
                .ToList();

            result.Add(new BulkPrintPreflightDto(
                patient.Id.Value,
                patient.FullName,
                verified.Any(pt => pt.IsPrinted),
                verified.Count,
                tests.Count));
        }

        return Task.FromResult(Result<IReadOnlyList<BulkPrintPreflightDto>>.Success(result));
    }
}
