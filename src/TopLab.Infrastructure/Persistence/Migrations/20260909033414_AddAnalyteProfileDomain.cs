using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopLab.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// M-05 Slice 1 schema + backfill. Creates the Analyte-owned reference-range
    /// model (Decision 1), the Profile/FixedPrice catalog (Decision 2), the frozen
    /// profile-item snapshot and the immutable amendment audit (Decision 3), then
    /// backfills from the legacy model:
    /// - one Analyte (+ one AnalyteReferenceRange aggregate with its typed bands) per
    ///   existing simple Test, copying the existing ReferenceRange bands verbatim;
    /// - Tests.AnalyteId mapping for simple tests through the deterministic
    ///   configured identity (analyte Name = test Name);
    /// - one Profile per existing specialised-profile Test with
    ///   FixedPrice = Test.PatientPrice;
    /// - legacy ProfileResultItems.AnalyteName re-pointed to the configured Analyte
    ///   only through the same deterministic Name match.
    /// If any legacy ProfileResultItem cannot be mapped deterministically the
    /// migration STOPS (THROW) — no clinical identity is ever guessed.
    /// Legacy ReferenceRange rows remain untouched for M-04 compatibility only; no
    /// new result path reads them.
    /// </summary>
    public partial class AddAnalyteProfileDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnalyteId",
                table: "Tests",
                type: "int",
                nullable: true);

            // Nullable during migration: legacy rows are backfilled below, then the
            // column is tightened to NOT NULL with the FK attached.
            migrationBuilder.AddColumn<int>(
                name: "AnalyteId",
                table: "ProfileResultItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPrintedAtUtc",
                table: "ProfileResultItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastPrintedByUserId",
                table: "ProfileResultItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrintCount",
                table: "ProfileResultItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Analytes",
                columns: table => new
                {
                    AnalyteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ReportName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedByUserId = table.Column<int>(type: "int", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analytes", x => x.AnalyteId);
                });

            migrationBuilder.CreateTable(
                name: "ProfileResultAmendments",
                columns: table => new
                {
                    ProfileResultAmendmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfileResultItemId = table.Column<int>(type: "int", nullable: false),
                    AmendedByUserId = table.Column<int>(type: "int", nullable: false),
                    AmendedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OldResultValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OldUnit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    OldFlag = table.Column<byte>(type: "tinyint", nullable: true),
                    NewResultValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NewUnit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NewFlag = table.Column<byte>(type: "tinyint", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileResultAmendments", x => x.ProfileResultAmendmentId);
                    table.ForeignKey(
                        name: "FK_ProfileResultAmendments_ProfileResultItems_ProfileResultItemId",
                        column: x => x.ProfileResultItemId,
                        principalTable: "ProfileResultItems",
                        principalColumn: "ProfileResultItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileResultItemReferenceRangeSnapshots",
                columns: table => new
                {
                    ProfileResultItemId = table.Column<int>(type: "int", nullable: false),
                    AnalyteId = table.Column<int>(type: "int", nullable: false),
                    Sex = table.Column<byte>(type: "tinyint", nullable: true),
                    AgeUnit = table.Column<byte>(type: "tinyint", nullable: false),
                    AgeMin = table.Column<int>(type: "int", nullable: false),
                    AgeMax = table.Column<int>(type: "int", nullable: false),
                    MinValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MaxValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LowComment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HighComment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CapturedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileResultItemReferenceRangeSnapshots", x => x.ProfileResultItemId);
                    table.ForeignKey(
                        name: "FK_ProfileResultItemReferenceRangeSnapshots_ProfileResultItems_ProfileResultItemId",
                        column: x => x.ProfileResultItemId,
                        principalTable: "ProfileResultItems",
                        principalColumn: "ProfileResultItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    ProfileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TestId = table.Column<int>(type: "int", nullable: false),
                    FixedPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedByUserId = table.Column<int>(type: "int", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.ProfileId);
                    table.ForeignKey(
                        name: "FK_Profiles_Tests_TestId",
                        column: x => x.TestId,
                        principalTable: "Tests",
                        principalColumn: "TestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalyteReferenceRanges",
                columns: table => new
                {
                    AnalyteReferenceRangeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnalyteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyteReferenceRanges", x => x.AnalyteReferenceRangeId);
                    table.ForeignKey(
                        name: "FK_AnalyteReferenceRanges_Analytes_AnalyteId",
                        column: x => x.AnalyteId,
                        principalTable: "Analytes",
                        principalColumn: "AnalyteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileAnalytes",
                columns: table => new
                {
                    ProfileAnalyteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfileId = table.Column<int>(type: "int", nullable: false),
                    AnalyteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileAnalytes", x => x.ProfileAnalyteId);
                    table.ForeignKey(
                        name: "FK_ProfileAnalytes_Analytes_AnalyteId",
                        column: x => x.AnalyteId,
                        principalTable: "Analytes",
                        principalColumn: "AnalyteId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfileAnalytes_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "ProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalyteReferenceRangeBands",
                columns: table => new
                {
                    AnalyteReferenceRangeBandId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnalyteReferenceRangeId = table.Column<int>(type: "int", nullable: false),
                    Sex = table.Column<byte>(type: "tinyint", nullable: true),
                    AgeUnit = table.Column<byte>(type: "tinyint", nullable: false),
                    AgeMin = table.Column<int>(type: "int", nullable: false),
                    AgeMax = table.Column<int>(type: "int", nullable: false),
                    MinValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MaxValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LowComment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HighComment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyteReferenceRangeBands", x => x.AnalyteReferenceRangeBandId);
                    table.ForeignKey(
                        name: "FK_AnalyteReferenceRangeBands_AnalyteReferenceRanges_AnalyteReferenceRangeId",
                        column: x => x.AnalyteReferenceRangeId,
                        principalTable: "AnalyteReferenceRanges",
                        principalColumn: "AnalyteReferenceRangeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tests_AnalyteId",
                table: "Tests",
                column: "AnalyteId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileResultItems_AnalyteId",
                table: "ProfileResultItems",
                column: "AnalyteId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyteReferenceRangeBands_AnalyteReferenceRangeId",
                table: "AnalyteReferenceRangeBands",
                column: "AnalyteReferenceRangeId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyteReferenceRanges_AnalyteId",
                table: "AnalyteReferenceRanges",
                column: "AnalyteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfileAnalytes_AnalyteId",
                table: "ProfileAnalytes",
                column: "AnalyteId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileAnalytes_ProfileId_AnalyteId",
                table: "ProfileAnalytes",
                columns: new[] { "ProfileId", "AnalyteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfileResultAmendments_ProfileResultItemId",
                table: "ProfileResultAmendments",
                column: "ProfileResultItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_Name",
                table: "Profiles",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_TestId",
                table: "Profiles",
                column: "TestId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileResultItems_Analytes_AnalyteId",
                table: "ProfileResultItems",
                column: "AnalyteId",
                principalTable: "Analytes",
                principalColumn: "AnalyteId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tests_Analytes_AnalyteId",
                table: "Tests",
                column: "AnalyteId",
                principalTable: "Analytes",
                principalColumn: "AnalyteId",
                onDelete: ReferentialAction.Restrict);

            // ── M-05 backfill ──────────────────────────────────────────────────────────
            // One Analyte per existing simple test. Name is the configured identity;
            // MIN(ReportName) keeps the insert deterministic even if legacy test names
            // repeat. The unique Name index is added after the backfill below.
            migrationBuilder.Sql(@"
INSERT INTO Analytes (Name, ReportName, IsActive, CreatedByUserId, CreatedAtUtc, LastModifiedByUserId, LastModifiedAtUtc, ModificationCount)
SELECT t.Name, MIN(t.ReportName), 1, 0, SYSDATETIME(), 0, SYSDATETIME(), 0
FROM Tests t
WHERE t.ResultKind = 0
GROUP BY t.Name;");

            // Map simple tests to their analyte through the deterministic Name match.
            migrationBuilder.Sql(@"
UPDATE t
SET t.AnalyteId = (SELECT MIN(a.AnalyteId) FROM Analytes a WHERE a.Name = t.Name)
FROM Tests t
WHERE t.ResultKind = 0;");

            // Exactly one current AnalyteReferenceRange aggregate per analyte.
            migrationBuilder.Sql(@"
INSERT INTO AnalyteReferenceRanges (AnalyteId)
SELECT AnalyteId FROM Analytes;");

            // Copy the legacy ReferenceRange bands verbatim into the analyte aggregate.
            migrationBuilder.Sql(@"
INSERT INTO AnalyteReferenceRangeBands (AnalyteReferenceRangeId, Sex, AgeUnit, AgeMin, AgeMax, MinValue, MaxValue, LowComment, HighComment)
SELECT arr.AnalyteReferenceRangeId, rr.Sex, rr.AgeUnit, rr.AgeMin, rr.AgeMax, rr.MinValue, rr.MaxValue, rr.LowComment, rr.HighComment
FROM ReferenceRanges rr
JOIN Tests t ON t.TestId = rr.TestId AND t.ResultKind = 0
JOIN Analytes a ON a.Name = t.Name
JOIN AnalyteReferenceRanges arr ON arr.AnalyteId = a.AnalyteId;");

            // One Profile per existing specialised-profile Test; FixedPrice = PatientPrice.
            migrationBuilder.Sql(@"
INSERT INTO Profiles (Name, TestId, FixedPrice, IsActive, CreatedByUserId, CreatedAtUtc, LastModifiedByUserId, LastModifiedAtUtc, ModificationCount)
SELECT t.Name, t.TestId, t.PatientPrice, 1, 0, SYSDATETIME(), 0, SYSDATETIME(), 0
FROM Tests t
WHERE t.ResultKind = 1;");

            // Map legacy profile items only through the deterministic configured
            // identity (analyte Name = stored AnalyteName).
            migrationBuilder.Sql(@"
UPDATE pri
SET pri.AnalyteId = a.AnalyteId
FROM ProfileResultItems pri
JOIN Analytes a ON a.Name = pri.AnalyteName;");

            // Mandatory stop: any legacy ProfileResultItem that still cannot be mapped
            // deterministically halts migration. Never guess clinical identity.
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM ProfileResultItems WHERE AnalyteId IS NULL)
    THROW 50001, N'M-05 backfill stopped: legacy ProfileResultItem rows have no deterministic configured Analyte match. An execution addendum is required; no clinical identity was guessed.', 1;");

            migrationBuilder.Sql(@"
ALTER TABLE ProfileResultItems ALTER COLUMN AnalyteId int NOT NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_Analytes_Name",
                table: "Analytes",
                column: "Name",
                unique: true);

            // Legacy name storage is replaced by the required AnalyteId. Only now, after
            // the backfill uses AnalyteName as its source, is the column removed.
            migrationBuilder.DropColumn(
                name: "AnalyteName",
                table: "ProfileResultItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfileResultItems_Analytes_AnalyteId",
                table: "ProfileResultItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Tests_Analytes_AnalyteId",
                table: "Tests");

            // Reconstruct the legacy name storage from the configured identity before
            // dropping the new model (existing result rows must stay readable).
            migrationBuilder.Sql(@"
UPDATE pri
SET pri.AnalyteName = a.Name
FROM ProfileResultItems pri
JOIN Analytes a ON a.AnalyteId = pri.AnalyteId;");

            migrationBuilder.DropTable(
                name: "AnalyteReferenceRangeBands");

            migrationBuilder.DropTable(
                name: "ProfileAnalytes");

            migrationBuilder.DropTable(
                name: "ProfileResultAmendments");

            migrationBuilder.DropTable(
                name: "ProfileResultItemReferenceRangeSnapshots");

            migrationBuilder.DropTable(
                name: "AnalyteReferenceRanges");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "Analytes");

            migrationBuilder.DropIndex(
                name: "IX_Tests_AnalyteId",
                table: "Tests");

            migrationBuilder.DropIndex(
                name: "IX_ProfileResultItems_AnalyteId",
                table: "ProfileResultItems");

            migrationBuilder.DropColumn(
                name: "AnalyteId",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "AnalyteId",
                table: "ProfileResultItems");

            migrationBuilder.DropColumn(
                name: "LastPrintedAtUtc",
                table: "ProfileResultItems");

            migrationBuilder.DropColumn(
                name: "LastPrintedByUserId",
                table: "ProfileResultItems");

            migrationBuilder.DropColumn(
                name: "PrintCount",
                table: "ProfileResultItems");

            migrationBuilder.AddColumn<string>(
                name: "AnalyteName",
                table: "ProfileResultItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }
    }
}