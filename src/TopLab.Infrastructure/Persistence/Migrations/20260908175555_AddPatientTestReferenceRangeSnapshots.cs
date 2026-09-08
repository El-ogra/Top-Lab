using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientTestReferenceRangeSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatientTestReferenceRangeSnapshots",
                columns: table => new
                {
                    PatientTestId = table.Column<int>(type: "int", nullable: false),
                    TestId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_PatientTestReferenceRangeSnapshots", x => x.PatientTestId);
                    table.ForeignKey(
                        name: "FK_PatientTestReferenceRangeSnapshots_PatientTests_PatientTestId",
                        column: x => x.PatientTestId,
                        principalTable: "PatientTests",
                        principalColumn: "PatientTestId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientTestReferenceRangeSnapshots");
        }
    }
}
