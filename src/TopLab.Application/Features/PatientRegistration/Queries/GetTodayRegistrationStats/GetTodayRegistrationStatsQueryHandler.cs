using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Billing;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.PatientRegistration.Queries.GetTodayRegistrationStats;

public sealed class GetTodayRegistrationStatsQueryHandler
    : IRequestHandler<GetTodayRegistrationStatsQuery, Result<TodayRegistrationStatsDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _dateTime;

    public GetTodayRegistrationStatsQueryHandler(IApplicationDbContext db, IDateTimeProvider dateTime)
    {
        _db = db;
        _dateTime = dateTime;
    }

    public Task<Result<TodayRegistrationStatsDto>> Handle(GetTodayRegistrationStatsQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTime.UtcNow);
        var fromStart = today.ToDateTime(TimeOnly.MinValue);
        var toEndExclusive = today.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var patientIds = _db.Set<Patient>()
            .Where(p => !p.IsDeleted && p.RegistrationDateUtc >= fromStart && p.RegistrationDateUtc < toEndExclusive)
            .Select(p => p.Id.Value)
            .ToList();

        var patientCount = patientIds.Count;
        var testCount = _db.Set<PatientTest>().Count(pt => patientIds.Contains(pt.PatientId.Value));

        var revenue = _db.Set<PaymentOperation>()
            .Where(po => !po.IsVoided && po.OperationAtUtc >= fromStart && po.OperationAtUtc < toEndExclusive)
            .Sum(po => (decimal?)po.Amount) ?? 0m;

        return Task.FromResult(Result<TodayRegistrationStatsDto>.Success(
            new TodayRegistrationStatsDto(patientCount, testCount, revenue)));
    }
}