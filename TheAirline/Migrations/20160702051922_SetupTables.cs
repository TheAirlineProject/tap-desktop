using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TheAirline.Migrations
{
    public partial class SetupTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Continents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlCe:ValueGeneration", "True"),
                    Name = table.Column<string>(nullable: true),
                    Uid = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Continents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlCe:ValueGeneration", "True"),
                    Exchange = table.Column<double>(nullable: false),
                    From = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Symbol = table.Column<string>(nullable: true),
                    SymbolOnRight = table.Column<bool>(nullable: false),
                    To = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Difficulties",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlCe:ValueGeneration", "True"),
                    AiLevel = table.Column<double>(nullable: false),
                    LoanLevel = table.Column<double>(nullable: false),
                    MoneyLevel = table.Column<double>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    PassengersLevel = table.Column<double>(nullable: false),
                    PriceLevel = table.Column<double>(nullable: false),
                    StartDataLevel = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Difficulties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlCe:ValueGeneration", "True"),
                    ContinentId = table.Column<int>(nullable: true),
                    FuelIndex = table.Column<double>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Uid = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Regions_Continents_ContinentId",
                        column: x => x.ContinentId,
                        principalTable: "Continents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlCe:ValueGeneration", "True"),
                    AirportCodeDisplay = table.Column<int>(nullable: false),
                    AutoSave = table.Column<int>(nullable: false),
                    ClearStats = table.Column<int>(nullable: false),
                    CurrencyShorten = table.Column<bool>(nullable: false),
                    DifficultyDisplayId = table.Column<int>(nullable: true),
                    GameSpeed = table.Column<int>(nullable: false),
                    Language = table.Column<string>(nullable: true),
                    MailsOnAirlineRoutes = table.Column<bool>(nullable: false),
                    MailsOnBadWeather = table.Column<bool>(nullable: false),
                    MailsOnLandings = table.Column<bool>(nullable: false),
                    Mode = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Settings_Difficulties_DifficultyDisplayId",
                        column: x => x.DifficultyDisplayId,
                        principalTable: "Difficulties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlCe:ValueGeneration", "True"),
                    ContinentId = table.Column<int>(nullable: true),
                    DifficultyId = table.Column<int>(nullable: true),
                    Focus = table.Column<int>(nullable: false),
                    MajorAirports = table.Column<bool>(nullable: false),
                    NumOfOpponents = table.Column<int>(nullable: false),
                    Paused = table.Column<bool>(nullable: false),
                    RandomOpponents = table.Column<bool>(nullable: false),
                    RealData = table.Column<bool>(nullable: false),
                    RegionId = table.Column<int>(nullable: true),
                    SameRegion = table.Column<bool>(nullable: false),
                    StartYear = table.Column<int>(nullable: false),
                    UseDays = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Continents_ContinentId",
                        column: x => x.ContinentId,
                        principalTable: "Continents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Players_Difficulties_DifficultyId",
                        column: x => x.DifficultyId,
                        principalTable: "Difficulties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Players_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Regions_ContinentId",
                table: "Regions",
                column: "ContinentId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_ContinentId",
                table: "Players",
                column: "ContinentId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_DifficultyId",
                table: "Players",
                column: "DifficultyId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_RegionId",
                table: "Players",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_DifficultyDisplayId",
                table: "Settings",
                column: "DifficultyDisplayId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "Difficulties");

            migrationBuilder.DropTable(
                name: "Continents");
        }
    }
}
