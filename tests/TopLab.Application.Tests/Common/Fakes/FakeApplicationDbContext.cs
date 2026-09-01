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
    public List<PatientTest> PatientTests { get; } = new();
    public List<PaymentOperation> PaymentOperations { get; } = new();
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

        if (typeof(TEntity) == typeof(PatientTest))
        {
            return (IQueryable<TEntity>)(object)PatientTests.AsQueryable();
        }

        if (typeof(TEntity) == typeof(PaymentOperation))
        {
            return (IQueryable<TEntity>)(object)PaymentOperations.AsQueryable();
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

        return Enumerable.Empty<TEntity>().AsQueryable();
    }

    public void Add<TEntity>(TEntity entity) where TEntity : class
    {
        if (entity is User u) Users.Add(u);
        else if (entity is Permission p) Permissions.Add(p);
        else if (entity is UserPermissionGrant g) UserPermissionGrants.Add(g);
        else if (entity is Patient pat) Patients.Add(pat);
        else if (entity is Test t) Tests.Add(t);
        else if (entity is PatientTest pt) PatientTests.Add(pt);
        else if (entity is PaymentOperation po) PaymentOperations.Add(po);
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
        else if (entity is PatientTest pt) PatientTests.Remove(pt);
        else if (entity is PaymentOperation po) PaymentOperations.Remove(po);
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
        else throw new NotSupportedException($"Remove not supported for {typeof(TEntity).Name}");
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return Task.FromResult(1);
    }

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
}
