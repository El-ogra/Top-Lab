using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureAndAntibiotics.Common;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.CultureAndAntibiotics.Queries.GetCultureAntibiotics;

public sealed class GetCultureAntibioticsQueryHandler
    : IRequestHandler<GetCultureAntibioticsQuery, Result<CultureAntibioticListDto>>
{
    private readonly IApplicationDbContext _db;

    public GetCultureAntibioticsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<CultureAntibioticListDto>> Handle(
        GetCultureAntibioticsQuery request, CancellationToken cancellationToken)
    {
        var test = _db.Set<Test>().FirstOrDefault(t => t.Id.Value == request.TestId);
        if (test is null)
        {
            return Task.FromResult(Result<CultureAntibioticListDto>.Failure(
                Error.NotFound("التحليل غير موجود")));
        }

        if (!test.IsCultureType)
        {
            return Task.FromResult(Result<CultureAntibioticListDto>.Failure(
                Error.Validation("التحليل المحدد ليس مزرعة.")));
        }

        var attachments = _db.Set<CultureAntibioticAttachment>()
            .Where(a => a.TestId.Value == request.TestId)
            .ToList();

        var antibioticIds = attachments
            .Select(a => a.AntibioticId.Value)
            .Distinct()
            .ToList();

        var antibioticsById = _db.Set<Antibiotic>()
            .Where(a => antibioticIds.Contains(a.Id.Value))
            .ToDictionary(a => a.Id.Value, a => a);

        IReadOnlyList<AttachedAntibioticDto> attached = attachments
            .Where(a => antibioticsById.ContainsKey(a.AntibioticId.Value))
            .Select(a =>
            {
                var ab = antibioticsById[a.AntibioticId.Value];
                return new AttachedAntibioticDto(
                    ab.Id.Value,
                    ab.Name,
                    ab.IsPregnancyFlagged,
                    ab.IsChildrenFlagged);
            })
            .ToList();

        var dto = new CultureAntibioticListDto(
            test.Id.Value,
            test.Name,
            attached.Count,
            attached);

        return Task.FromResult(Result<CultureAntibioticListDto>.Success(dto));
    }
}