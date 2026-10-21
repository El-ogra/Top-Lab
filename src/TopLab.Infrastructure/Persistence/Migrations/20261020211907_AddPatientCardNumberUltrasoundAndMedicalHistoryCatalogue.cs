using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TopLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientCardNumberUltrasoundAndMedicalHistoryCatalogue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PrintResultPasswordEnabled",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PrintResultPasswordHash",
                table: "SystemSettings",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardNumber",
                table: "Patients",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RecentUltrasoundImaging",
                table: "Patients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "MedicalConditionTypes",
                columns: new[] { "MedicalConditionTypeId", "Category", "Name" },
                values: new object[,]
                {
                    { 2, (byte)0, "هل يؤخذ علاج للسكر" },
                    { 3, (byte)0, "هل يؤخذ علاج لسيولة الدم" },
                    { 4, (byte)0, "هل يؤخذ علاج للغدد" },
                    { 5, (byte)0, "هل يؤخذ علاج للفيروسات" },
                    { 6, (byte)0, "هل يؤخذ مضاد حيوي" },
                    { 7, (byte)0, "هل يؤخذ علاج للكبد" },
                    { 8, (byte)1, "مريض بفقر الدم" },
                    { 9, (byte)1, "مريض بالتهاب المفاصل" },
                    { 10, (byte)1, "مريض بالضغط الدم" },
                    { 11, (byte)1, "مريض بالفشل كلوي" },
                    { 12, (byte)1, "مريض بالذئبة الحمراء" }
                });

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SystemSettingsId",
                keyValue: 1,
                column: "PrintResultPasswordHash",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_CardNumber",
                table: "Patients",
                column: "CardNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patients_CardNumber",
                table: "Patients");

            // STAGE-01 rollback: PatientMedicalConditions carries no foreign key to
            // MedicalConditionTypes, so the join rows that reference the eleven rows
            // removed below are deleted first. They can only exist because this
            // migration created the catalogue entries, so no pre-existing patient data
            // is lost.
            migrationBuilder.Sql(@"
DELETE FROM PatientMedicalConditions
WHERE MedicalConditionTypeId IN (2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12);
");

            migrationBuilder.DeleteData(
                table: "MedicalConditionTypes",
                keyColumn: "MedicalConditionTypeId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MedicalConditionTypes",
                keyColumn: "MedicalConditionTypeId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MedicalConditionTypes",
                keyColumn: "MedicalConditionTypeId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MedicalConditionTypes",
                keyColumn: "MedicalConditionTypeId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "MedicalConditionTypes",
                keyColumn: "MedicalConditionTypeId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "MedicalConditionTypes",
                keyColumn: "MedicalConditionTypeId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "MedicalConditionTypes",
                keyColumn: "MedicalConditionTypeId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "MedicalConditionTypes",
                keyColumn: "MedicalConditionTypeId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "MedicalConditionTypes",
                keyColumn: "MedicalConditionTypeId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "MedicalConditionTypes",
                keyColumn: "MedicalConditionTypeId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "MedicalConditionTypes",
                keyColumn: "MedicalConditionTypeId",
                keyValue: 12);

            migrationBuilder.DropColumn(
                name: "PrintResultPasswordEnabled",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "PrintResultPasswordHash",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "CardNumber",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "RecentUltrasoundImaging",
                table: "Patients");
        }
    }
}
