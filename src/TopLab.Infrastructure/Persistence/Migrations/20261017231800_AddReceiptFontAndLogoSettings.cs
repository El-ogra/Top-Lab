using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptFontAndLogoSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FooterFontColor",
                table: "ReceiptSettings",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterFontFamily",
                table: "ReceiptSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FooterFontSizePt",
                table: "ReceiptSettings",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeaderFontColor",
                table: "ReceiptSettings",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeaderFontFamily",
                table: "ReceiptSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HeaderFontSizePt",
                table: "ReceiptSettings",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoImagePath",
                table: "ReceiptSettings",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ReceiptSettings",
                keyColumn: "ReceiptSettingsId",
                keyValue: 1,
                columns: new[] { "FooterFontColor", "FooterFontFamily", "FooterFontSizePt", "HeaderFontColor", "HeaderFontFamily", "HeaderFontSizePt", "LogoImagePath" },
                values: new object[] { null, null, null, null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FooterFontColor",
                table: "ReceiptSettings");

            migrationBuilder.DropColumn(
                name: "FooterFontFamily",
                table: "ReceiptSettings");

            migrationBuilder.DropColumn(
                name: "FooterFontSizePt",
                table: "ReceiptSettings");

            migrationBuilder.DropColumn(
                name: "HeaderFontColor",
                table: "ReceiptSettings");

            migrationBuilder.DropColumn(
                name: "HeaderFontFamily",
                table: "ReceiptSettings");

            migrationBuilder.DropColumn(
                name: "HeaderFontSizePt",
                table: "ReceiptSettings");

            migrationBuilder.DropColumn(
                name: "LogoImagePath",
                table: "ReceiptSettings");
        }
    }
}
