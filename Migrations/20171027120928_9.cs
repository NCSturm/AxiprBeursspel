using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Beursspel.Migrations
{
    public partial class _9 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HuidigeWaarde",
                table: "Beurzen");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "BeursWaardes",
                type: "int4",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "BeursWaardes");

            migrationBuilder.AddColumn<double>(
                name: "HuidigeWaarde",
                table: "Beurzen",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
