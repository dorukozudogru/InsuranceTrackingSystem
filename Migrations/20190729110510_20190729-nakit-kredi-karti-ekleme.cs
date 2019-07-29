using Microsoft.EntityFrameworkCore.Migrations;

namespace SigortaTakipSistemi.Migrations
{
    public partial class _20190729nakitkredikartiekleme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "InsurancePaymentType",
                table: "Insurances",
                nullable: false,
                defaultValue: (byte)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InsurancePaymentType",
                table: "Insurances");
        }
    }
}
