using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SigortaTakipSistemi.Migrations
{
    public partial class cancelinsuranceadded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Insurances",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Insurances");
        }
    }
}
