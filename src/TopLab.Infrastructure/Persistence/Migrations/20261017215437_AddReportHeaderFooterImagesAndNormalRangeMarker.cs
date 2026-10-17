using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReportHeaderFooterImagesAndNormalRangeMarker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FooterImagePath",
                table: "ReportSettings",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeaderImagePath",
                table: "ReportSettings",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalRangeMarker",
                table: "ReportSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ReportSettings",
                keyColumn: "ReportSettingsId",
                keyValue: 1,
                columns: new[] { "FooterImagePath", "HeaderImagePath", "NormalRangeMarker" },
                values: new object[] { null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FooterImagePath",
                table: "ReportSettings");

            migrationBuilder.DropColumn(
                name: "HeaderImagePath",
                table: "ReportSettings");

            migrationBuilder.DropColumn(
                name: "NormalRangeMarker",
                table: "ReportSettings");
        }
    }
}
