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

namespace TopLab.Infrastructure.Persistence;

public partial class ApplicationDbContext
{
    public Microsoft.EntityFrameworkCore.DbSet<Patient> Patients => Set<Patient>();
    public Microsoft.EntityFrameworkCore.DbSet<PatientPhoneNumber> PatientPhoneNumbers => Set<PatientPhoneNumber>();
    public Microsoft.EntityFrameworkCore.DbSet<MedicalConditionType> MedicalConditionTypes => Set<MedicalConditionType>();
    public Microsoft.EntityFrameworkCore.DbSet<PatientMedicalCondition> PatientMedicalConditions => Set<PatientMedicalCondition>();
    public Microsoft.EntityFrameworkCore.DbSet<PatientTitle> PatientTitles => Set<PatientTitle>();
    public Microsoft.EntityFrameworkCore.DbSet<TestGroup> TestGroups => Set<TestGroup>();
    public Microsoft.EntityFrameworkCore.DbSet<Test> Tests => Set<Test>();
    public Microsoft.EntityFrameworkCore.DbSet<ReferenceRange> ReferenceRanges => Set<ReferenceRange>();
    public Microsoft.EntityFrameworkCore.DbSet<TestComment> TestComments => Set<TestComment>();
    public Microsoft.EntityFrameworkCore.DbSet<CustomGroup> CustomGroups => Set<CustomGroup>();
    public Microsoft.EntityFrameworkCore.DbSet<CustomGroupItem> CustomGroupItems => Set<CustomGroupItem>();
    public Microsoft.EntityFrameworkCore.DbSet<WorkGroupLog> WorkGroupLogs => Set<WorkGroupLog>();
    public Microsoft.EntityFrameworkCore.DbSet<WorkGroupLogItem> WorkGroupLogItems => Set<WorkGroupLogItem>();
    public Microsoft.EntityFrameworkCore.DbSet<Antibiotic> Antibiotics => Set<Antibiotic>();
    public Microsoft.EntityFrameworkCore.DbSet<CultureAntibioticAttachment> CultureAntibioticAttachments => Set<CultureAntibioticAttachment>();
    public Microsoft.EntityFrameworkCore.DbSet<PatientTest> PatientTests => Set<PatientTest>();
    public Microsoft.EntityFrameworkCore.DbSet<ProfileResultItem> ProfileResultItems => Set<ProfileResultItem>();
    public Microsoft.EntityFrameworkCore.DbSet<CultureResult> CultureResults => Set<CultureResult>();
    public Microsoft.EntityFrameworkCore.DbSet<CultureAntibioticResult> CultureAntibioticResults => Set<CultureAntibioticResult>();
    public Microsoft.EntityFrameworkCore.DbSet<PaymentOperation> PaymentOperations => Set<PaymentOperation>();
    public Microsoft.EntityFrameworkCore.DbSet<PriceList> PriceLists => Set<PriceList>();
    public Microsoft.EntityFrameworkCore.DbSet<PriceListItem> PriceListItems => Set<PriceListItem>();
    public Microsoft.EntityFrameworkCore.DbSet<ExternalEntity> ExternalEntities => Set<ExternalEntity>();
    public Microsoft.EntityFrameworkCore.DbSet<SentOutSample> SentOutSamples => Set<SentOutSample>();
    public Microsoft.EntityFrameworkCore.DbSet<SentOutSamplePayment> SentOutSamplePayments => Set<SentOutSamplePayment>();
    public Microsoft.EntityFrameworkCore.DbSet<User> Users => Set<User>();
    public Microsoft.EntityFrameworkCore.DbSet<Permission> Permissions => Set<Permission>();
    public Microsoft.EntityFrameworkCore.DbSet<UserPermissionGrant> UserPermissionGrants => Set<UserPermissionGrant>();
    public Microsoft.EntityFrameworkCore.DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public Microsoft.EntityFrameworkCore.DbSet<CashMovement> CashMovements => Set<CashMovement>();
    public Microsoft.EntityFrameworkCore.DbSet<SystemSettings> SystemSettings => Set<SystemSettings>();
    public Microsoft.EntityFrameworkCore.DbSet<ReportSettings> ReportSettings => Set<ReportSettings>();
    public Microsoft.EntityFrameworkCore.DbSet<ReceiptSettings> ReceiptSettings => Set<ReceiptSettings>();
    public Microsoft.EntityFrameworkCore.DbSet<EnvelopeSettings> EnvelopeSettings => Set<EnvelopeSettings>();
    public Microsoft.EntityFrameworkCore.DbSet<EnvelopePrintItemPosition> EnvelopePrintItemPositions => Set<EnvelopePrintItemPosition>();
    public Microsoft.EntityFrameworkCore.DbSet<PrinterAssignment> PrinterAssignments => Set<PrinterAssignment>();
}
