using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopLab.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRefundAmountToPaymentOperation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "PaymentOperations",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "PaymentOperations");
        }
    }
}
