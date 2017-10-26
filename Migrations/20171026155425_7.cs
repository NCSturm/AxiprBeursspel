using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Beursspel.Migrations
{
    public partial class _7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bewerker",
                table: "TelMomenten",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LaatstBewerkt",
                table: "TelMomenten",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bewerker",
                table: "TelMomenten");

            migrationBuilder.DropColumn(
                name: "LaatstBewerkt",
                table: "TelMomenten");
        }
    }
}
