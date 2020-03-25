using Microsoft.EntityFrameworkCore.Migrations;

namespace SigortaTakipSistemi.Migrations
{
    public partial class cancelinsuranceadded_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CancelledInsuranceAmount",
                table: "Insurances",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CancelledInsuranceBonus",
                table: "Insurances",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledInsuranceAmount",
                table: "Insurances");

            migrationBuilder.DropColumn(
                name: "CancelledInsuranceBonus",
                table: "Insurances");
        }
    }
}
