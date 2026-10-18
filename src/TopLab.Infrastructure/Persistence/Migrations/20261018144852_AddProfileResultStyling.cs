using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileResultStyling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AbnormalBackgroundColor",
                table: "Profiles",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AbnormalFont",
                table: "Profiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalColor",
                table: "Profiles",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalFont",
                table: "Profiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AbnormalBackgroundColor",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "AbnormalFont",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "NormalColor",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "NormalFont",
                table: "Profiles");
        }
    }
}
