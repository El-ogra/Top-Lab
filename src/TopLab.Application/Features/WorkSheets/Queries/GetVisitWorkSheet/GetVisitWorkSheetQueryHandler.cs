using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.WorkSheets.Queries.GetVisitWorkSheet;

public sealed class GetVisitWorkSheetQueryHandler : IRequestHandler<GetVisitWorkSheetQuery, Result<VisitWorkSheetDto>>
{
    private const int UngroupedSectionId = 0;
    private const string UngroupedSectionName = "بدون مجموعة";

    private readonly IApplicationDbContext _db;

    public GetVisitWorkSheetQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<VisitWorkSheetDto>> Handle(GetVisitWorkSheetQuery request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Task.FromResult(Result<VisitWorkSheetDto>.Failure(Error.NotFound("المريض غير موجود.")));
        }

        var settings = _db.Set<SystemSettings>().SingleOrDefault(s => s.Id == 1);
        if (settings is null)
        {
            return Task.FromResult(Result<VisitWorkSheetDto>.Failure(Error.Unexpected("سجل الإعدادات العامة مفقود.")));
        }

        var (lines, samples) = WorkSheetVisitLines.SelectVisit(_db, patient);

        var testIdByLine = _db.Set<PatientTest>()
            .Where(pt => pt.PatientId.Value == patient.Id.Value)
            .ToDictionary(pt => pt.Id.Value, pt => pt.TestId.Value);

        var testsById = _db.Set<Test>().ToDictionary(t => t.Id.Value);
        var sections = _db.Set<TestGroup>()
            .Where(g => g.IsActive)
            .OrderBy(g => g.Name)
            .ToList()
            .Select(g => new WorkSheetSectionDto(
                g.Id.Value,
                g.Name,
                lines.Where(l => testIdByLine.TryGetValue(l.PatientTestId, out var testId)
                    && testsById.TryGetValue(testId, out var t)
                    && t.TestGroupId != null
                    && t.TestGroupId.Value == g.Id.Value).ToList()))
            .Where(s => s.Lines.Count > 0)
            .ToList();

        var ungrouped = lines.Where(l => !testIdByLine.TryGetValue(l.PatientTestId, out var testId)
            || !testsById.TryGetValue(testId, out var t)
            || t.TestGroupId == null).ToList();
        if (ungrouped.Count > 0)
        {
            sections.Add(new WorkSheetSectionDto(UngroupedSectionId, UngroupedSectionName, ungrouped));
        }

        var sheet = new VisitWorkSheetDto(
            patient.Id.Value,
            patient.FullName,
            patient.LabId == null ? null : patient.LabId.Value,
            sections,
            samples,
            lines.Count,
            settings.PrintFileExternalBarcode,
            settings.PrintDateTimeOnTubeBarcode,
            settings.PrintLabIdInsteadOfPatientId);

        return Task.FromResult(Result<VisitWorkSheetDto>.Success(sheet));
    }
}
