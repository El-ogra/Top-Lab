using TopLab.Application.Common.Interfaces;
using TopLab.Domain.Accounting;
using TopLab.Domain.Attendance;
using TopLab.Domain.Billing;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.SentOutSamples;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;
using TopLab.Domain.Users;

namespace TopLab.Application.Tests.Common.Fakes;

public sealed class FakeApplicationDbContext : IApplicationDbContext
{
    public List<User> Users { get; } = new();
    public List<Permission> Permissions { get; } = new();
    public List<UserPermissionGrant> UserPermissionGrants { get; } = new();
    public List<Patient> Patients { get; } = new();
    public List<Test> Tests { get; } = new();
    public List<TestGroup> TestGroups { get; } = new();
    public List<ReferenceRange> ReferenceRanges { get; } = new();
    public List<Analyte> Analytes { get; } = new();
    public List<AnalyteReferenceRange> AnalyteReferenceRanges { get; } = new();
    public List<AnalyteReferenceRangeBand> AnalyteReferenceRangeBands { get; } = new();
    public List<Profile> Profiles { get; } = new();
    public List<ProfileAnalyte> ProfileAnalytes { get; } = new();
    public List<WorkGroupLog> WorkGroupLogs { get; } = new();
    public List<WorkGroupLogItem> WorkGroupLogItems { get; } = new();
    public List<TestComment> TestComments { get; } = new();
    public List<PatientTest> PatientTests { get; } = new();
    public List<PaymentOperation> PaymentOperations { get; } = new();
    public List<PriceList> PriceLists { get; } = new();
    public List<PriceListItem> PriceListItems { get; } = new();
    public List<CustomGroup> CustomGroups { get; } = new();
    public List<CustomGroupItem> CustomGroupItems { get; } = new();
    public List<CashMovement> CashMovements { get; } = new();
    public List<ExternalEntity> ExternalEntities { get; } = new();
    public List<SentOutSample> SentOutSamples { get; } = new();
    public List<AttendanceRecord> AttendanceRecords { get; } = new();
    public List<SystemSettings> SystemSettings { get; } = new();
    public List<ReportSettings> ReportSettings { get; } = new();
    public List<ReceiptSettings> ReceiptSettings { get; } = new();
    public List<EnvelopeSettings> EnvelopeSettings { get; } = new();
    public List<EnvelopePrintItemPosition> EnvelopePrintItemPositions { get; } = new();
    public List<PrinterAssignment> PrinterAssignments { get; } = new();
    public List<Antibiotic> Antibiotics { get; } = new();
    public List<CultureAntibioticAttachment> CultureAntibioticAttachments { get; } = new();
    public List<CultureAntibioticResult> CultureAntibioticResults { get; } = new();
    public List<PatientPhoneNumber> PatientPhoneNumbers { get; } = new();
    public List<PatientMedicalCondition> PatientMedicalConditions { get; } = new();
    public List<PatientTitle> PatientTitles { get; } = new();
    public List<MedicalConditionType> MedicalConditionTypes { get; } = new();
    public List<PatientTestReferenceRangeSnapshot> PatientTestReferenceRangeSnapshots { get; } = new();
    public List<ProfileResultItem> ProfileResultItems { get; } = new();
    public List<ProfileResultItemReferenceRangeSnapshot> ProfileResultItemReferenceRangeSnapshots { get; } = new();
    public List<ProfileResultAmendment> ProfileResultAmendments { get; } = new();
    public List<CultureResult> CultureResults { get; } = new();

    public int SaveChangesCallCount { get; private set; }

    public IQueryable<TEntity> Set<TEntity>() where TEntity : class
    {
        if (typeof(TEntity) == typeof(User))
        {
            return (IQueryable<TEntity>)(object)Users.AsQueryable();
        }

        if (typeof(TEntity) == typeof(Permission))
        {
            return (IQueryable<TEntity>)(object)Permissions.AsQueryable();
        }

        if (typeof(TEntity) == typeof(UserPermissionGrant))
        {
            return (IQueryable<TEntity>)(object)UserPermissionGrants.AsQueryable();
        }

        if (typeof(TEntity) == typeof(Patient))
        {
            return (IQueryable<TEntity>)(object)Patients.AsQueryable();
        }

        if (typeof(TEntity) == typeof(Test))
        {
            return (IQueryable<TEntity>)(object)Tests.AsQueryable();
        }

        if (typeof(TEntity) == typeof(TestGroup))
        {
            return (IQueryable<TEntity>)(object)TestGroups.AsQueryable();
        }

        if (typeof(TEntity) == typeof(ReferenceRange))
        {
            return (IQueryable<TEntity>)(object)ReferenceRanges.AsQueryable();
        }

        if (typeof(TEntity) == typeof(Analyte))
        {
            return (IQueryable<TEntity>)(object)Analytes.AsQueryable();
        }

        if (typeof(TEntity) == typeof(AnalyteReferenceRange))
        {
            return (IQueryable<TEntity>)(object)AnalyteReferenceRanges.AsQueryable();
        }

        if (typeof(TEntity) == typeof(AnalyteReferenceRangeBand))
        {
            return (IQueryable<TEntity>)(object)AnalyteReferenceRangeBands.AsQueryable();
        }

        if (typeof(TEntity) == typeof(Profile))
        {
            return (IQueryable<TEntity>)(object)Profiles.AsQueryable();
        }

        if (typeof(TEntity) == typeof(ProfileAnalyte))
        {
            return (IQueryable<TEntity>)(object)ProfileAnalytes.AsQueryable();
        }

        if (typeof(TEntity) == typeof(WorkGroupLog))
        {
            return (IQueryable<TEntity>)(object)WorkGroupLogs.AsQueryable();
        }

        if (typeof(TEntity) == typeof(WorkGroupLogItem))
        {
            return (IQueryable<TEntity>)(object)WorkGroupLogItems.AsQueryable();
        }

        if (typeof(TEntity) == typeof(TestComment))
        {
            return (IQueryable<TEntity>)(object)TestComments.AsQueryable();
        }

        if (typeof(TEntity) == typeof(PatientTest))
        {
            return (IQueryable<TEntity>)(object)PatientTests.AsQueryable();
        }

        if (typeof(TEntity) == typeof(PaymentOperation))
        {
            return (IQueryable<TEntity>)(object)PaymentOperations.AsQueryable();
        }

        if (typeof(TEntity) == typeof(PriceList))
        {
            return (IQueryable<TEntity>)(object)PriceLists.AsQueryable();
        }

        if (typeof(TEntity) == typeof(PriceListItem))
        {
            return (IQueryable<TEntity>)(object)PriceListItems.AsQueryable();
        }

        if (typeof(TEntity) == typeof(CustomGroup))
        {
            return (IQueryable<TEntity>)(object)CustomGroups.AsQueryable();
        }

        if (typeof(TEntity) == typeof(CustomGroupItem))
        {
            return (IQueryable<TEntity>)(object)CustomGroupItems.AsQueryable();
        }

        if (typeof(TEntity) == typeof(CashMovement))
        {
            return (IQueryable<TEntity>)(object)CashMovements.AsQueryable();
        }

        if (typeof(TEntity) == typeof(ExternalEntity))
        {
            return (IQueryable<TEntity>)(object)ExternalEntities.AsQueryable();
        }

        if (typeof(TEntity) == typeof(SentOutSample))
        {
            return (IQueryable<TEntity>)(object)SentOutSamples.AsQueryable();
        }

        if (typeof(TEntity) == typeof(AttendanceRecord))
        {
            return (IQueryable<TEntity>)(object)AttendanceRecords.AsQueryable();
        }

        if (typeof(TEntity) == typeof(SystemSettings))
        {
            return (IQueryable<TEntity>)(object)SystemSettings.AsQueryable();
        }

        if (typeof(TEntity) == typeof(ReportSettings))
        {
            return (IQueryable<TEntity>)(object)ReportSettings.AsQueryable();
        }

        if (typeof(TEntity) == typeof(ReceiptSettings))
        {
            return (IQueryable<TEntity>)(object)ReceiptSettings.AsQueryable();
        }

        if (typeof(TEntity) == typeof(EnvelopeSettings))
        {
            return (IQueryable<TEntity>)(object)EnvelopeSettings.AsQueryable();
        }

        if (typeof(TEntity) == typeof(EnvelopePrintItemPosition))
        {
            return (IQueryable<TEntity>)(object)EnvelopePrintItemPositions.AsQueryable();
        }

        if (typeof(TEntity) == typeof(PrinterAssignment))
        {
            return (IQueryable<TEntity>)(object)PrinterAssignments.AsQueryable();
        }

        if (typeof(TEntity) == typeof(Antibiotic))
        {
            return (IQueryable<TEntity>)(object)Antibiotics.AsQueryable();
        }

        if (typeof(TEntity) == typeof(CultureAntibioticAttachment))
        {
            return (IQueryable<TEntity>)(object)CultureAntibioticAttachments.AsQueryable();
        }

        if (typeof(TEntity) == typeof(CultureAntibioticResult))
        {
            return (IQueryable<TEntity>)(object)CultureAntibioticResults.AsQueryable();
        }

        if (typeof(TEntity) == typeof(PatientPhoneNumber))
        {
            return (IQueryable<TEntity>)(object)PatientPhoneNumbers.AsQueryable();
        }

        if (typeof(TEntity) == typeof(PatientMedicalCondition))
        {
            return (IQueryable<TEntity>)(object)PatientMedicalConditions.AsQueryable();
        }

        if (typeof(TEntity) == typeof(PatientTitle))
        {
            return (IQueryable<TEntity>)(object)PatientTitles.AsQueryable();
        }

        if (typeof(TEntity) == typeof(MedicalConditionType))
        {
            return (IQueryable<TEntity>)(object)MedicalConditionTypes.AsQueryable();
        }

        if (typeof(TEntity) == typeof(PatientTestReferenceRangeSnapshot))
        {
            return (IQueryable<TEntity>)(object)PatientTestReferenceRangeSnapshots.AsQueryable();
        }

        if (typeof(TEntity) == typeof(ProfileResultItem))
        {
            return (IQueryable<TEntity>)(object)ProfileResultItems.AsQueryable();
        }

        if (typeof(TEntity) == typeof(ProfileResultItemReferenceRangeSnapshot))
        {
            return (IQueryable<TEntity>)(object)ProfileResultItemReferenceRangeSnapshots.AsQueryable();
        }

        if (typeof(TEntity) == typeof(ProfileResultAmendment))
        {
            return (IQueryable<TEntity>)(object)ProfileResultAmendments.AsQueryable();
        }

        if (typeof(TEntity) == typeof(CultureResult))
        {
            return (IQueryable<TEntity>)(object)CultureResults.AsQueryable();
        }

        return Enumerable.Empty<TEntity>().AsQueryable();
    }

    public void Add<TEntity>(TEntity entity) where TEntity : class
    {
        if (entity is User u) Users.Add(u);
        else if (entity is Permission p) Permissions.Add(p);
        else if (entity is UserPermissionGrant g) UserPermissionGrants.Add(g);
        else if (entity is Patient pat) Patients.Add(pat);
        else if (entity is Test t) Tests.Add(t);
        else if (entity is TestGroup tg) TestGroups.Add(tg);
        else if (entity is ReferenceRange rr) ReferenceRanges.Add(rr);
        else if (entity is Analyte an) Analytes.Add(an);
        else if (entity is AnalyteReferenceRange arr) AnalyteReferenceRanges.Add(arr);
        else if (entity is AnalyteReferenceRangeBand arrb) AnalyteReferenceRangeBands.Add(arrb);
        else if (entity is Profile prof) Profiles.Add(prof);
        else if (entity is ProfileAnalyte paar) ProfileAnalytes.Add(paar);
        else if (entity is WorkGroupLog wgl) WorkGroupLogs.Add(wgl);
        else if (entity is WorkGroupLogItem wgli) WorkGroupLogItems.Add(wgli);
        else if (entity is TestComment tc) TestComments.Add(tc);
        else if (entity is PatientTest pt) PatientTests.Add(pt);
        else if (entity is PaymentOperation po) PaymentOperations.Add(po);
        else if (entity is PriceList pl) PriceLists.Add(pl);
        else if (entity is PriceListItem pli) PriceListItems.Add(pli);
        else if (entity is CustomGroup cg) CustomGroups.Add(cg);
        else if (entity is CustomGroupItem cgi) CustomGroupItems.Add(cgi);
        else if (entity is CashMovement cm) CashMovements.Add(cm);
        else if (entity is ExternalEntity ee) ExternalEntities.Add(ee);
        else if (entity is SentOutSample sos) SentOutSamples.Add(sos);
        else if (entity is AttendanceRecord ar) AttendanceRecords.Add(ar);
        else if (entity is SystemSettings sys) SystemSettings.Add(sys);
        else if (entity is ReportSettings rep) ReportSettings.Add(rep);
        else if (entity is ReceiptSettings rec) ReceiptSettings.Add(rec);
        else if (entity is EnvelopeSettings env) EnvelopeSettings.Add(env);
        else if (entity is EnvelopePrintItemPosition epos) EnvelopePrintItemPositions.Add(epos);
        else if (entity is PrinterAssignment pa) PrinterAssignments.Add(pa);
        else if (entity is Antibiotic ab) Antibiotics.Add(ab);
        else if (entity is CultureAntibioticAttachment caa) CultureAntibioticAttachments.Add(caa);
        else if (entity is CultureAntibioticResult car) CultureAntibioticResults.Add(car);
        else if (entity is PatientPhoneNumber ppn) PatientPhoneNumbers.Add(ppn);
        else if (entity is PatientMedicalCondition pmc) PatientMedicalConditions.Add(pmc);
        else if (entity is PatientTitle pt2) PatientTitles.Add(pt2);
        else if (entity is MedicalConditionType mct) MedicalConditionTypes.Add(mct);
        else if (entity is PatientTestReferenceRangeSnapshot snap) PatientTestReferenceRangeSnapshots.Add(snap);
        else if (entity is ProfileResultItem pri) ProfileResultItems.Add(pri);
        else if (entity is ProfileResultItemReferenceRangeSnapshot pris) ProfileResultItemReferenceRangeSnapshots.Add(pris);
        else if (entity is ProfileResultAmendment pra) ProfileResultAmendments.Add(pra);
        else if (entity is CultureResult cr) CultureResults.Add(cr);
        else throw new NotSupportedException($"Add not supported for {typeof(TEntity).Name}");
    }

    public void Update<TEntity>(TEntity entity) where TEntity : class
    {
        // In-memory list: no action needed, entity is already reference-tracked.
    }

    public void Remove<TEntity>(TEntity entity) where TEntity : class
    {
        if (entity is User u) Users.Remove(u);
        else if (entity is Permission p) Permissions.Remove(p);
        else if (entity is UserPermissionGrant g) UserPermissionGrants.Remove(g);
        else if (entity is Patient pat) Patients.Remove(pat);
        else if (entity is Test t) Tests.Remove(t);
        else if (entity is TestGroup tg) TestGroups.Remove(tg);
        else if (entity is ReferenceRange rr) ReferenceRanges.Remove(rr);
        else if (entity is Analyte an) Analytes.Remove(an);
        else if (entity is AnalyteReferenceRange arr) AnalyteReferenceRanges.Remove(arr);
        else if (entity is AnalyteReferenceRangeBand arrb) AnalyteReferenceRangeBands.Remove(arrb);
        else if (entity is Profile prof) Profiles.Remove(prof);
        else if (entity is ProfileAnalyte paar) ProfileAnalytes.Remove(paar);
        else if (entity is WorkGroupLog wgl) WorkGroupLogs.Remove(wgl);
        else if (entity is WorkGroupLogItem wgli) WorkGroupLogItems.Remove(wgli);
        else if (entity is TestComment tc) TestComments.Remove(tc);
        else if (entity is PatientTest pt) PatientTests.Remove(pt);
        else if (entity is PaymentOperation po) PaymentOperations.Remove(po);
        else if (entity is PriceList pl) PriceLists.Remove(pl);
        else if (entity is PriceListItem pli) PriceListItems.Remove(pli);
        else if (entity is CustomGroup cg) CustomGroups.Remove(cg);
        else if (entity is CustomGroupItem cgi) CustomGroupItems.Remove(cgi);
        else if (entity is CashMovement cm) CashMovements.Remove(cm);
        else if (entity is ExternalEntity ee) ExternalEntities.Remove(ee);
        else if (entity is SentOutSample sos) SentOutSamples.Remove(sos);
        else if (entity is AttendanceRecord ar) AttendanceRecords.Remove(ar);
        else if (entity is SystemSettings sys) SystemSettings.Remove(sys);
        else if (entity is ReportSettings rep) ReportSettings.Remove(rep);
        else if (entity is ReceiptSettings rec) ReceiptSettings.Remove(rec);
        else if (entity is EnvelopeSettings env) EnvelopeSettings.Remove(env);
        else if (entity is EnvelopePrintItemPosition epos) EnvelopePrintItemPositions.Remove(epos);
        else if (entity is PrinterAssignment pa) PrinterAssignments.Remove(pa);
        else if (entity is Antibiotic ab) Antibiotics.Remove(ab);
        else if (entity is CultureAntibioticAttachment caa) CultureAntibioticAttachments.Remove(caa);
        else if (entity is CultureAntibioticResult car) CultureAntibioticResults.Remove(car);
        else if (entity is PatientPhoneNumber ppn) PatientPhoneNumbers.Remove(ppn);
        else if (entity is PatientMedicalCondition pmc) PatientMedicalConditions.Remove(pmc);
        else if (entity is PatientTitle pt2) PatientTitles.Remove(pt2);
        else if (entity is MedicalConditionType mct) MedicalConditionTypes.Remove(mct);
        else if (entity is PatientTestReferenceRangeSnapshot snap) PatientTestReferenceRangeSnapshots.Remove(snap);
        else if (entity is ProfileResultItem pri) ProfileResultItems.Remove(pri);
        else if (entity is ProfileResultItemReferenceRangeSnapshot pris) ProfileResultItemReferenceRangeSnapshots.Remove(pris);
        else if (entity is ProfileResultAmendment pra) ProfileResultAmendments.Remove(pra);
        else if (entity is CultureResult cr) CultureResults.Remove(cr);
        else throw new NotSupportedException($"Remove not supported for {typeof(TEntity).Name}");
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return Task.FromResult(1);
    }

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
}
