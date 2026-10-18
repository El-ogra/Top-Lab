using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReportItemLayout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LeftOffsetCm",
                table: "ReportItemTemplates",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LineSpacing",
                table: "ReportItemTemplates",
                type: "decimal(4,2)",
                precision: 4,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarkerSymbol",
                table: "ReportItemTemplates",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "CultureLine",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "CultureSensitivityGrid",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "GroupSubtitle",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "HighComment",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "HistoryComment",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "HistoryDate",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "HistoryResult",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "HistoryReviewed",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "HistoryTest",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "LowComment",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "MicroscopyLine",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "OffLabNote",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "PatientExportCultureSummary",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "PatientExportProfileItems",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "PatientExportTestLine",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "ProfileAnalyte",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "ProfileHighComment",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "ProfileLowComment",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "ProfileReferenceRange",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "ProfileReportComment",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "ReferenceRange",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "ResultValue",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "TestComment",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "ReportItemTemplates",
                keyColumn: "ItemKey",
                keyValue: "TestName",
                columns: new[] { "LeftOffsetCm", "LineSpacing", "MarkerSymbol" },
                values: new object[] { null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeftOffsetCm",
                table: "ReportItemTemplates");

            migrationBuilder.DropColumn(
                name: "LineSpacing",
                table: "ReportItemTemplates");

            migrationBuilder.DropColumn(
                name: "MarkerSymbol",
                table: "ReportItemTemplates");
        }
    }
}
