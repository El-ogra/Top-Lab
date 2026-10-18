using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAbnormalResultColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AbnormalResultBackgroundColor",
                table: "ReportSettings",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AbnormalResultFontColor",
                table: "ReportSettings",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableAbnormalResultColor",
                table: "ReportSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "ReportSettings",
                keyColumn: "ReportSettingsId",
                keyValue: 1,
                columns: new[] { "AbnormalResultBackgroundColor", "AbnormalResultFontColor" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AbnormalResultBackgroundColor",
                table: "ReportSettings");

            migrationBuilder.DropColumn(
                name: "AbnormalResultFontColor",
                table: "ReportSettings");

            migrationBuilder.DropColumn(
                name: "EnableAbnormalResultColor",
                table: "ReportSettings");
        }
    }
}
