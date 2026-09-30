using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopLab.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// WP-03 / SD-1: repair culture sensitivity off-by-one.
    /// Repair map (authoritative): 0→NULL · 1→0 · 2→1 · 3→2.
    /// Pre-write report (read-only, run before applying):
    ///   SELECT SensitivityCategory, COUNT(*) FROM CultureAntibioticResults GROUP BY SensitivityCategory;
    /// Post-write report:
    ///   SELECT SensitivityCategory, COUNT(*) FROM CultureAntibioticResults GROUP BY SensitivityCategory;
    /// Backup table CultureSensitivityBackup is retained (never dropped).
    /// C-6: column becomes nullable so Unspecified (formerly 0) can be stored as NULL.
    /// </summary>
    public partial class FixCultureSensitivityCategoryOffByOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID('CultureSensitivityBackup', 'U') IS NULL
BEGIN
    SELECT * INTO CultureSensitivityBackup FROM CultureAntibioticResults;
END
");

            migrationBuilder.AlterColumn<byte>(
                name: "SensitivityCategory",
                table: "CultureAntibioticResults",
                type: "tinyint",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            // SD-1 repair map. Order matters: 3→2 must run before 2→1, etc. Use CASE in one statement.
            migrationBuilder.Sql(@"
UPDATE CultureAntibioticResults
SET SensitivityCategory = CASE SensitivityCategory
    WHEN 0 THEN NULL
    WHEN 1 THEN 0
    WHEN 2 THEN 1
    WHEN 3 THEN 2
    ELSE SensitivityCategory
END;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore from backup when possible (best-effort; backup is never dropped).
            migrationBuilder.Sql(@"
IF OBJECT_ID('CultureSensitivityBackup', 'U') IS NOT NULL
BEGIN
    DELETE FROM CultureAntibioticResults;
    INSERT INTO CultureAntibioticResults (CultureAntibioticResultId, PatientTestId, AntibioticId, SensitivityCategory)
    SELECT CultureAntibioticResultId, PatientTestId, AntibioticId, SensitivityCategory
    FROM CultureSensitivityBackup;
END
");

            migrationBuilder.AlterColumn<byte>(
                name: "SensitivityCategory",
                table: "CultureAntibioticResults",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldNullable: true);
        }
    }
}
