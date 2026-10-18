using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TopLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReportItemTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportItemTemplates",
                columns: table => new
                {
                    ItemKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReferenceValueExpression = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ShowHide = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ListMembership = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FreeTextOverride = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportItemTemplates", x => x.ItemKey);
                });

            migrationBuilder.InsertData(
                table: "ReportItemTemplates",
                columns: new[] { "ItemKey", "Label", "ListMembership", "ReferenceValueExpression", "ShowHide" },
                values: new object[,]
                {
                    { "CultureLine", "المزرعة", null, null, true },
                    { "CultureSensitivityGrid", "جدول الحساسية", null, null, true },
                    { "GroupSubtitle", "المجموعة", null, null, true },
                    { "HighComment", "تعليق القيمة المرتفعة", null, null, true },
                    { "HistoryComment", "التعليق", null, null, true },
                    { "HistoryDate", "التاريخ", null, null, true },
                    { "HistoryResult", "النتيجة", null, null, true },
                    { "HistoryReviewed", "معتمد", null, null, true },
                    { "HistoryTest", "التحليل", null, null, true },
                    { "LowComment", "تعليق القيمة المنخفضة", null, null, true },
                    { "MicroscopyLine", "الفحص المجهري", null, null, true },
                    { "OffLabNote", "العينة أُخذت خارج المعمل", null, null, true },
                    { "PatientExportCultureSummary", "ملخص المزرعة", null, null, true },
                    { "PatientExportProfileItems", "عناصر التحليل", null, null, true },
                    { "PatientExportTestLine", "التحليل", null, null, true },
                    { "ProfileAnalyte", "التحليل", null, null, true },
                    { "ProfileHighComment", "تعليق مرتفع للمحلل", null, null, true },
                    { "ProfileLowComment", "تعليق منخفض للمحلل", null, null, true },
                    { "ProfileReferenceRange", "المدى", null, null, true },
                    { "ProfileReportComment", "تعليق التقرير", null, null, true },
                    { "ReferenceRange", "المدى", null, null, true },
                    { "ResultValue", "النتيجة", null, null, true },
                    { "TestComment", "تعليق التحليل", null, null, true },
                    { "TestName", "التحليل", null, null, true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportItemTemplates");
        }
    }
}
