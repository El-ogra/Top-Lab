using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAntibioticMasterFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ScientificName",
                table: "Antibiotics",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Symbol",
                table: "Antibiotics",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScientificName",
                table: "Antibiotics");

            migrationBuilder.DropColumn(
                name: "Symbol",
                table: "Antibiotics");
        }
    }
}
