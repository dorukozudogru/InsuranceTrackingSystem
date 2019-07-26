using Microsoft.EntityFrameworkCore.Migrations;

namespace SigortaTakipSistemi.Migrations
{
    public partial class changeinsurancecompanytable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoPath",
                table: "InsuranceCompanies",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoPath",
                table: "InsuranceCompanies");
        }
    }
}
