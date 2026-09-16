using System.Globalization;
using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientRegistration.Queries.GetNextLabId;

public sealed class GetNextLabIdQueryHandler : IRequestHandler<GetNextLabIdQuery, Result<string>>
{
    private readonly IApplicationDbContext _db;

    public GetNextLabIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<string>> Handle(GetNextLabIdQuery request, CancellationToken cancellationToken)
    {
        var numeric = _db.Set<Patient>()
            .Where(p => p.LabId != null)
            .Select(p => p.LabId!.Value)
            .ToList()
            .Where(v => v.Length > 0 && v.All(char.IsDigit))
            .ToList();

        if (numeric.Count == 0)
        {
            return Task.FromResult(Result<string>.Success("1"));
        }

        var width = numeric.Max(v => v.Length);
        var max = numeric.Select(v => long.Parse(v, CultureInfo.InvariantCulture)).Max();
        var next = (max + 1).ToString(CultureInfo.InvariantCulture).PadLeft(width, '0');
        return Task.FromResult(Result<string>.Success(next));
    }
}
