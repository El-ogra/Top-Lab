using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPregnancyMedicalConditionTypeSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MedicalConditionTypes",
                columns: new[] { "MedicalConditionTypeId", "Category", "Name" },
                values: new object[] { 1, (byte)2, "حمل" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MedicalConditionTypes",
                keyColumn: "MedicalConditionTypeId",
                keyValue: 1);
        }
    }
}
