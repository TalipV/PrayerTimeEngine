﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrayerTimeEngine.Core.Data.EntityFramework.Generated_Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FaziletCountries",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false),
                    InsertInstant = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaziletCountries", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "FaziletPrayerTimes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<string>(type: "TEXT", nullable: false),
                    CityID = table.Column<int>(type: "INTEGER", nullable: false),
                    InsertInstant = table.Column<string>(type: "TEXT", nullable: true),
                    Imsak = table.Column<string>(type: "TEXT", nullable: false),
                    Fajr = table.Column<string>(type: "TEXT", nullable: false),
                    Shuruq = table.Column<string>(type: "TEXT", nullable: false),
                    Dhuhr = table.Column<string>(type: "TEXT", nullable: false),
                    Asr = table.Column<string>(type: "TEXT", nullable: false),
                    Maghrib = table.Column<string>(type: "TEXT", nullable: false),
                    Isha = table.Column<string>(type: "TEXT", nullable: false),
                    NextFajr = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaziletPrayerTimes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MuwaqqitPrayerTimes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<string>(type: "TEXT", nullable: false),
                    Longitude = table.Column<decimal>(type: "TEXT", nullable: false),
                    Latitude = table.Column<decimal>(type: "TEXT", nullable: false),
                    InsertInstant = table.Column<string>(type: "TEXT", nullable: true),
                    FajrDegree = table.Column<double>(type: "REAL", nullable: false),
                    AsrKarahaDegree = table.Column<double>(type: "REAL", nullable: false),
                    IshtibaqDegree = table.Column<double>(type: "REAL", nullable: false),
                    IshaDegree = table.Column<double>(type: "REAL", nullable: false),
                    Fajr = table.Column<string>(type: "TEXT", nullable: false),
                    NextFajr = table.Column<string>(type: "TEXT", nullable: false),
                    Shuruq = table.Column<string>(type: "TEXT", nullable: false),
                    Duha = table.Column<string>(type: "TEXT", nullable: false),
                    Dhuhr = table.Column<string>(type: "TEXT", nullable: false),
                    Asr = table.Column<string>(type: "TEXT", nullable: false),
                    AsrMithlayn = table.Column<string>(type: "TEXT", nullable: false),
                    Maghrib = table.Column<string>(type: "TEXT", nullable: false),
                    Isha = table.Column<string>(type: "TEXT", nullable: false),
                    Ishtibaq = table.Column<string>(type: "TEXT", nullable: false),
                    AsrKaraha = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuwaqqitPrayerTimes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SemerkandCountries",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false),
                    InsertInstant = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SemerkandCountries", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SemerkandPrayerTimes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DayOfYear = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<string>(type: "TEXT", nullable: false),
                    CityID = table.Column<int>(type: "INTEGER", nullable: false),
                    InsertInstant = table.Column<string>(type: "TEXT", nullable: true),
                    Fajr = table.Column<string>(type: "TEXT", nullable: false),
                    Shuruq = table.Column<string>(type: "TEXT", nullable: false),
                    Dhuhr = table.Column<string>(type: "TEXT", nullable: false),
                    Asr = table.Column<string>(type: "TEXT", nullable: false),
                    Maghrib = table.Column<string>(type: "TEXT", nullable: false),
                    Isha = table.Column<string>(type: "TEXT", nullable: false),
                    NextFajr = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SemerkandPrayerTimes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TimezoneInfos",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InsertInstant = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    UtcOffsetSeconds = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimezoneInfos", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "FaziletCities",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false),
                    InsertInstant = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    CountryID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaziletCities", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FaziletCities_FaziletCountries_CountryID",
                        column: x => x.CountryID,
                        principalTable: "FaziletCountries",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SemerkandCities",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false),
                    InsertInstant = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    CountryID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SemerkandCities", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SemerkandCities_SemerkandCountries_CountryID",
                        column: x => x.CountryID,
                        principalTable: "SemerkandCountries",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaceInfos",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InsertInstant = table.Column<string>(type: "TEXT", nullable: true),
                    TimezoneInfoID = table.Column<int>(type: "INTEGER", nullable: true),
                    ExternalID = table.Column<string>(type: "TEXT", nullable: true),
                    Longitude = table.Column<decimal>(type: "TEXT", nullable: false),
                    Latitude = table.Column<decimal>(type: "TEXT", nullable: false),
                    InfoLanguageCode = table.Column<string>(type: "TEXT", nullable: true),
                    Country = table.Column<string>(type: "TEXT", nullable: true),
                    City = table.Column<string>(type: "TEXT", nullable: true),
                    CityDistrict = table.Column<string>(type: "TEXT", nullable: true),
                    PostCode = table.Column<string>(type: "TEXT", nullable: true),
                    Street = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaceInfos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PlaceInfos_TimezoneInfos_TimezoneInfoID",
                        column: x => x.TimezoneInfoID,
                        principalTable: "TimezoneInfos",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    PlaceInfoID = table.Column<int>(type: "INTEGER", nullable: true),
                    SequenceNo = table.Column<int>(type: "INTEGER", nullable: false),
                    InsertInstant = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Profiles_PlaceInfos_PlaceInfoID",
                        column: x => x.PlaceInfoID,
                        principalTable: "PlaceInfos",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "ProfileConfigs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InsertInstant = table.Column<string>(type: "TEXT", nullable: true),
                    ProfileID = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeType = table.Column<int>(type: "INTEGER", nullable: false),
                    CalculationConfiguration = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileConfigs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ProfileConfigs_Profiles_ProfileID",
                        column: x => x.ProfileID,
                        principalTable: "Profiles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileLocations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InsertInstant = table.Column<string>(type: "TEXT", nullable: true),
                    ProfileID = table.Column<int>(type: "INTEGER", nullable: false),
                    CalculationSource = table.Column<int>(type: "INTEGER", nullable: false),
                    LocationData = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileLocations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ProfileLocations_Profiles_ProfileID",
                        column: x => x.ProfileID,
                        principalTable: "Profiles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FaziletCities_CountryID",
                table: "FaziletCities",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_PlaceInfos_TimezoneInfoID",
                table: "PlaceInfos",
                column: "TimezoneInfoID");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileConfigs_ProfileID",
                table: "ProfileConfigs",
                column: "ProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileLocations_ProfileID",
                table: "ProfileLocations",
                column: "ProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_PlaceInfoID",
                table: "Profiles",
                column: "PlaceInfoID");

            migrationBuilder.CreateIndex(
                name: "IX_SemerkandCities_CountryID",
                table: "SemerkandCities",
                column: "CountryID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaziletCities");

            migrationBuilder.DropTable(
                name: "FaziletPrayerTimes");

            migrationBuilder.DropTable(
                name: "MuwaqqitPrayerTimes");

            migrationBuilder.DropTable(
                name: "ProfileConfigs");

            migrationBuilder.DropTable(
                name: "ProfileLocations");

            migrationBuilder.DropTable(
                name: "SemerkandCities");

            migrationBuilder.DropTable(
                name: "SemerkandPrayerTimes");

            migrationBuilder.DropTable(
                name: "FaziletCountries");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "SemerkandCountries");

            migrationBuilder.DropTable(
                name: "PlaceInfos");

            migrationBuilder.DropTable(
                name: "TimezoneInfos");
        }
    }
}
