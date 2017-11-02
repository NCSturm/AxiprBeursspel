using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Beursspel.Migrations
{
    public partial class _14 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOpen",
                table: "Settings",
                type: "bool",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "StartBeursBeschikbareAandelen",
                table: "Settings",
                type: "int4",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "StartBeursWaarde",
                table: "Settings",
                type: "float8",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StartSpelerGeld",
                table: "Settings",
                type: "float8",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOpen",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "StartBeursBeschikbareAandelen",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "StartBeursWaarde",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "StartSpelerGeld",
                table: "Settings");
        }
    }
}
