using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCultureMicroscopyAndZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InhibitionZoneMm",
                table: "CultureAntibioticResults",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SensitivityThresholdMm",
                table: "CultureAntibioticAttachments",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CultureMicroscopies",
                columns: table => new
                {
                    PatientTestId = table.Column<int>(type: "int", nullable: false),
                    PusCells = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RedBloodCells = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EpithelialCells = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Crystals = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Fungi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OthersOne = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OthersTwo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OthersThree = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsDirect = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CultureMicroscopies", x => x.PatientTestId);
                    table.ForeignKey(
                        name: "FK_CultureMicroscopies_CultureResults_PatientTestId",
                        column: x => x.PatientTestId,
                        principalTable: "CultureResults",
                        principalColumn: "PatientTestId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CultureMicroscopies");

            migrationBuilder.DropColumn(
                name: "InhibitionZoneMm",
                table: "CultureAntibioticResults");

            migrationBuilder.DropColumn(
                name: "SensitivityThresholdMm",
                table: "CultureAntibioticAttachments");
        }
    }
}
