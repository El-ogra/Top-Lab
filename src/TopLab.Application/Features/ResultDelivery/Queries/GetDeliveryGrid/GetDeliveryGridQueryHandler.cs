using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.ResultDelivery.Queries.GetDeliveryGrid;

public sealed class GetDeliveryGridQueryHandler
    : IRequestHandler<GetDeliveryGridQuery, Result<IReadOnlyList<DeliveryGridRowDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetDeliveryGridQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<DeliveryGridRowDto>>> Handle(
        GetDeliveryGridQuery request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Task.FromResult(Result<IReadOnlyList<DeliveryGridRowDto>>.Failure(
                Error.NotFound("المريض غير موجود.")));
        }

        var catalog = _db.Set<Test>().ToDictionary(t => t.Id.Value);

        IReadOnlyList<DeliveryGridRowDto> rows = _db.Set<PatientTest>()
            .Where(pt => pt.PatientId.Value == request.PatientId)
            .OrderBy(pt => pt.Id.Value)
            .ToList()
            .Select(pt =>
            {
                catalog.TryGetValue(pt.TestId.Value, out var test);
                return new DeliveryGridRowDto(
                    pt.Id.Value,
                    test?.Name ?? string.Empty,
                    test?.TestCode ?? string.Empty,
                    pt.ResultValue,
                    pt.ResultFlag == null ? null : (int)pt.ResultFlag.Value,
                    test == null ? 0 : (int)test.ResultKind,
                    pt.EnteredAtUtc is not null,
                    pt.IsReviewed,
                    pt.IsPrinted,
                    pt.IsDelivered,
                    pt.PriceAtOrderTime);
            }).ToList();

        return Task.FromResult(Result<IReadOnlyList<DeliveryGridRowDto>>.Success(rows));
    }
}
