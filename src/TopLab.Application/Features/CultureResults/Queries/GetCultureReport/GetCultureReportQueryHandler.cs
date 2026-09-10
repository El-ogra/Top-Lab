using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureResults.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.CultureResults.Queries.GetCultureReport;

public sealed class GetCultureReportQueryHandler : IRequestHandler<GetCultureReportQuery, Result<CultureReportDto>>
{
    private readonly IApplicationDbContext _db;
    public GetCultureReportQueryHandler(IApplicationDbContext db) => _db = db;

    public Task<Result<CultureReportDto>> Handle(GetCultureReportQuery request, CancellationToken cancellationToken)
    {
        var pt = _db.Set<PatientTest>().FirstOrDefault(x => x.Id.Value == request.PatientTestId);
        if (pt is null) return Task.FromResult(Result<CultureReportDto>.Failure(Error.NotFound("التحليل غير موجود")));
        var test = _db.Set<Test>().FirstOrDefault(x => x.Id.Value == pt.TestId.Value);
        if (test is null) return Task.FromResult(Result<CultureReportDto>.Failure(Error.NotFound("التحليل غير موجود")));
        if (!test.IsCultureType) return Task.FromResult(Result<CultureReportDto>.Failure(Error.Conflict("هذا التحليل ليس من نوع المزارع.")));
        var patient = _db.Set<Patient>().FirstOrDefault(x => x.Id.Value == pt.PatientId.Value);
        if (patient is null || patient.IsDeleted) return Task.FromResult(Result<CultureReportDto>.Failure(Error.NotFound("المريض غير موجود.")));
        var settings = _db.Set<SystemSettings>().FirstOrDefault();
        if (settings is null) return Task.FromResult(Result<CultureReportDto>.Failure(Error.Unexpected("سجل الإعدادات العامة مفقود.")));
        var antibiotics = _db.Set<Antibiotic>().ToDictionary(x => x.Id.Value);
        var rows = _db.Set<CultureAntibioticResult>().Where(x => x.PatientTestId.Value == pt.Id.Value).OrderBy(x => x.AntibioticId.Value).ToList().Select(x =>
        {
            antibiotics.TryGetValue(x.AntibioticId.Value, out var antibiotic);
            return new CultureSensitivityRowDto(x.Id.Value, x.AntibioticId.Value, antibiotic?.Name ?? $"[{x.AntibioticId.Value}]", (int)x.SensitivityCategory, antibiotic?.IsPregnancyFlagged ?? false, antibiotic?.IsChildrenFlagged ?? false);
        }).ToList();
        var header = _db.Set<CultureResult>().FirstOrDefault(x => x.PatientTestId.Value == pt.Id.Value);
        return Task.FromResult(Result<CultureReportDto>.Success(new CultureReportDto(pt.Id.Value, patient.Id.Value, patient.FullName, patient.LabId?.Value, patient.Sex.ToString(), patient.AgeValue, patient.AgeUnit.ToString(), test.Name, test.ReportName, patient.RegistrationDateUtc, header?.Sample, header?.OrganismA, header?.OrganismB, header?.OrganismC, header?.CultureCondition, header?.ColonyCount, rows, settings.PrintLabIdInsteadOfPatientId, pt.IsReviewed, pt.IsPrinted, pt.PrintCount)));
    }
}
