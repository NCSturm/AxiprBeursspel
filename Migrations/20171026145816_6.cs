using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Beursspel.Migrations
{
    public partial class _6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TelMomenten",
                columns: table => new
                {
                    TelMomentHouderId = table.Column<int>(type: "int4", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Invoerder = table.Column<string>(type: "text", nullable: true),
                    Tijd = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelMomenten", x => x.TelMomentHouderId);
                });

            migrationBuilder.CreateTable(
                name: "TelMomentModel",
                columns: table => new
                {
                    TelMomentModelId = table.Column<int>(type: "int4", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Aantal = table.Column<int>(type: "int4", nullable: false),
                    BeursId = table.Column<int>(type: "int4", nullable: false),
                    BeursNaam = table.Column<string>(type: "text", nullable: true),
                    TelMomentHouderId = table.Column<int>(type: "int4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelMomentModel", x => x.TelMomentModelId);
                    table.ForeignKey(
                        name: "FK_TelMomentModel_TelMomenten_TelMomentHouderId",
                        column: x => x.TelMomentHouderId,
                        principalTable: "TelMomenten",
                        principalColumn: "TelMomentHouderId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TelMomentModel_TelMomentHouderId",
                table: "TelMomentModel",
                column: "TelMomentHouderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TelMomentModel");

            migrationBuilder.DropTable(
                name: "TelMomenten");
        }
    }
}
