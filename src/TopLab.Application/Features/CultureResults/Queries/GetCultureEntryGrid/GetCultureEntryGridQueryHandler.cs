using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureAndAntibiotics.Common;
using TopLab.Application.Features.CultureResults.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.CultureResults.Queries.GetCultureEntryGrid;

public sealed class GetCultureEntryGridQueryHandler : IRequestHandler<GetCultureEntryGridQuery, Result<CultureEntryGridDto>>
{
    private readonly IApplicationDbContext _db;
    public GetCultureEntryGridQueryHandler(IApplicationDbContext db) => _db = db;

    public Task<Result<CultureEntryGridDto>> Handle(GetCultureEntryGridQuery request, CancellationToken cancellationToken)
    {
        var pt = _db.Set<PatientTest>().FirstOrDefault(x => x.Id.Value == request.PatientTestId);
        if (pt is null) return Task.FromResult(Result<CultureEntryGridDto>.Failure(Error.NotFound("التحليل غير موجود")));
        var test = _db.Set<Test>().FirstOrDefault(x => x.Id.Value == pt.TestId.Value);
        if (test is null) return Task.FromResult(Result<CultureEntryGridDto>.Failure(Error.NotFound("التحليل غير موجود")));
        if (!test.IsCultureType) return Task.FromResult(Result<CultureEntryGridDto>.Failure(Error.Conflict("هذا التحليل ليس من نوع المزارع.")));
        var patient = _db.Set<Patient>().FirstOrDefault(x => x.Id.Value == pt.PatientId.Value);
        if (patient is null || patient.IsDeleted) return Task.FromResult(Result<CultureEntryGridDto>.Failure(Error.NotFound("المريض غير موجود.")));
        var categories = from conditionJoin in _db.Set<PatientMedicalCondition>()
                         where conditionJoin.PatientId.Value == patient.Id.Value
                         join conditionType in _db.Set<MedicalConditionType>()
                             on conditionJoin.MedicalConditionTypeId.Value equals conditionType.Id.Value
                         select conditionType.Category;
        var pregnant = PregnancySignal.IsPregnancyIndicated(categories);
        var child = patient.AgeUnit == AgeUnit.Year && patient.AgeValue < CultureAntibioticDisplay.ChildAgeThresholdYears;
        var saved = _db.Set<CultureAntibioticResult>().Where(x => x.PatientTestId.Value == pt.Id.Value).ToDictionary(x => x.AntibioticId.Value);
        var attachedIds = _db.Set<CultureAntibioticAttachment>().Where(x => x.TestId.Value == test.Id.Value).Select(x => x.AntibioticId.Value).ToHashSet();
        var antibiotics = _db.Set<Antibiotic>().Where(x => attachedIds.Contains(x.Id.Value)).ToList();
        var rows = antibiotics.Where(x => CultureAntibioticDisplay.IsDisplayable(x.IsPregnancyFlagged, x.IsChildrenFlagged, pregnant, child) || saved.ContainsKey(x.Id.Value))
            .OrderBy(x => x.Id.Value).Select(x => { saved.TryGetValue(x.Id.Value, out var item); return new CultureSensitivityRowDto(item?.Id.Value, x.Id.Value, x.Name, item is null ? null : (int)item.SensitivityCategory, x.IsPregnancyFlagged, x.IsChildrenFlagged); }).ToList();
        var header = _db.Set<CultureResult>().FirstOrDefault(x => x.PatientTestId.Value == pt.Id.Value);
        return Task.FromResult(Result<CultureEntryGridDto>.Success(new CultureEntryGridDto(pt.Id.Value, patient.Id.Value, test.Name, test.TestCode, header?.Sample, header?.OrganismA, header?.OrganismB, header?.OrganismC, header?.CultureCondition, header?.ColonyCount, rows, pt.IsReviewed, pt.IsPrinted)));
    }
}
