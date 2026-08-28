using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenamePkColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "WorkGroupLogs",
                newName: "WorkGroupLogId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Tests",
                newName: "TestId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TestGroups",
                newName: "TestGroupId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TestComments",
                newName: "TestCommentId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SystemSettings",
                newName: "SystemSettingsId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SentOutSamples",
                newName: "SentOutSampleId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SentOutSamplePayments",
                newName: "SentOutSamplePaymentId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ReportSettings",
                newName: "ReportSettingsId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ReferenceRanges",
                newName: "ReferenceRangeId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ReceiptSettings",
                newName: "ReceiptSettingsId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ProfileResultItems",
                newName: "ProfileResultItemId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PriceLists",
                newName: "PriceListId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Permissions",
                newName: "PermissionId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PaymentOperations",
                newName: "PaymentOperationId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PatientTitles",
                newName: "PatientTitleId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PatientTests",
                newName: "PatientTestId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Patients",
                newName: "PatientId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PatientPhoneNumbers",
                newName: "PatientPhoneNumberId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "MedicalConditionTypes",
                newName: "MedicalConditionTypeId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ExternalEntities",
                newName: "ExternalEntityId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "EnvelopeSettings",
                newName: "EnvelopeSettingsId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "CustomGroups",
                newName: "CustomGroupId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "CultureAntibioticResults",
                newName: "CultureAntibioticResultId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "CashMovements",
                newName: "CashMovementId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AttendanceRecords",
                newName: "AttendanceRecordId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Antibiotics",
                newName: "AntibioticId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WorkGroupLogId",
                table: "WorkGroupLogs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "TestId",
                table: "Tests",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "TestGroupId",
                table: "TestGroups",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "TestCommentId",
                table: "TestComments",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "SystemSettingsId",
                table: "SystemSettings",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "SentOutSampleId",
                table: "SentOutSamples",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "SentOutSamplePaymentId",
                table: "SentOutSamplePayments",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ReportSettingsId",
                table: "ReportSettings",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ReferenceRangeId",
                table: "ReferenceRanges",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ReceiptSettingsId",
                table: "ReceiptSettings",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ProfileResultItemId",
                table: "ProfileResultItems",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PriceListId",
                table: "PriceLists",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PermissionId",
                table: "Permissions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PaymentOperationId",
                table: "PaymentOperations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PatientTitleId",
                table: "PatientTitles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PatientTestId",
                table: "PatientTests",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "Patients",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PatientPhoneNumberId",
                table: "PatientPhoneNumbers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "MedicalConditionTypeId",
                table: "MedicalConditionTypes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ExternalEntityId",
                table: "ExternalEntities",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "EnvelopeSettingsId",
                table: "EnvelopeSettings",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "CustomGroupId",
                table: "CustomGroups",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "CultureAntibioticResultId",
                table: "CultureAntibioticResults",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "CashMovementId",
                table: "CashMovements",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "AttendanceRecordId",
                table: "AttendanceRecords",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "AntibioticId",
                table: "Antibiotics",
                newName: "Id");
        }
    }
}
