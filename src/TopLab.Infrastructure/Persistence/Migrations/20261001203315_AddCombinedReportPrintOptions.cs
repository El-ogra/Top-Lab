using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCombinedReportPrintOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PrintGroupSubTitle",
                table: "ReportSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SuppressReprintMessage",
                table: "ReportSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // W-02 S7 fixup (F1): explicit seed values — an empty UpdateData emits
            // "UPDATE [ReportSettings] SET WHERE ..." which is invalid T-SQL.
            migrationBuilder.UpdateData(
                table: "ReportSettings",
                keyColumn: "ReportSettingsId",
                keyValue: 1,
                columns: new[] { "PrintGroupSubTitle", "SuppressReprintMessage" },
                values: new object[] { false, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrintGroupSubTitle",
                table: "ReportSettings");

            migrationBuilder.DropColumn(
                name: "SuppressReprintMessage",
                table: "ReportSettings");
        }
    }
}
