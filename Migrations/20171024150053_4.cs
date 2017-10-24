using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Beursspel.Migrations
{
    public partial class _4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BeursWaardes",
                columns: table => new
                {
                    BeursWaardesId = table.Column<int>(type: "int4", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    BeursId = table.Column<int>(type: "int4", nullable: false),
                    Tijd = table.Column<DateTime>(type: "timestamp", nullable: false),
                    Waarde = table.Column<double>(type: "float8", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeursWaardes", x => x.BeursWaardesId);
                    table.ForeignKey(
                        name: "FK_BeursWaardes_Beurzen_BeursId",
                        column: x => x.BeursId,
                        principalTable: "Beurzen",
                        principalColumn: "BeursId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BeursWaardes_BeursId",
                table: "BeursWaardes",
                column: "BeursId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeursWaardes");
        }
    }
}
