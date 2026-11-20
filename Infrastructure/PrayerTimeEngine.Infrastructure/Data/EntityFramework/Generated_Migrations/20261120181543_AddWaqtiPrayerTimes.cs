using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrayerTimeEngine.Core.Data.EntityFramework.Generated_Migrations;

/// <inheritdoc />
public partial class AddWaqtiPrayerTimes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "WaqtiPrayerTimes",
            columns: table => new
            {
                ID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Date = table.Column<string>(type: "TEXT", nullable: false),
                TimeZone = table.Column<string>(type: "TEXT", nullable: false),
                Longitude = table.Column<decimal>(type: "TEXT", nullable: false),
                Latitude = table.Column<decimal>(type: "TEXT", nullable: false),
                Method = table.Column<string>(type: "TEXT", nullable: false),
                FajrAngle = table.Column<double>(type: "REAL", nullable: true),
                IshaaAngle = table.Column<double>(type: "REAL", nullable: true),
                InsertInstant = table.Column<string>(type: "TEXT", nullable: true),
                Fajr = table.Column<string>(type: "TEXT", nullable: true),
                Shuruq = table.Column<string>(type: "TEXT", nullable: true),
                Duha = table.Column<string>(type: "TEXT", nullable: true),
                Dhuhr = table.Column<string>(type: "TEXT", nullable: true),
                Asr = table.Column<string>(type: "TEXT", nullable: true),
                AsrMithlayn = table.Column<string>(type: "TEXT", nullable: true),
                Maghrib = table.Column<string>(type: "TEXT", nullable: true),
                Isha = table.Column<string>(type: "TEXT", nullable: true),
                NextFajr = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WaqtiPrayerTimes", x => x.ID);
            });

        // Waqti needs exactly the location data Muwaqqit has (coordinates and time zone), so
        // existing profiles get a copy of their Muwaqqit location data instead of no Waqti
        // location data until the next place selection. Only the type discriminator and the
        // provider value (Muwaqqit = 5, Waqti = 25) differ.
        migrationBuilder.Sql(
            """
            INSERT INTO ProfileLocations (InsertInstant, ProfileID, DynamicPrayerTimeProvider, LocationData)
            SELECT
                muwaqqit.InsertInstant,
                muwaqqit.ProfileID,
                25,
                replace(
                    replace(muwaqqit.LocationData, '"$type":"MuwaqqitLocationData"', '"$type":"WaqtiLocationData"'),
                    '"Source":5', '"Source":25')
            FROM ProfileLocations AS muwaqqit
            WHERE muwaqqit.DynamicPrayerTimeProvider = 5
              AND muwaqqit.LocationData LIKE '%"$type":"MuwaqqitLocationData"%'
              AND NOT EXISTS (
                  SELECT 1
                  FROM ProfileLocations AS waqti
                  WHERE waqti.ProfileID = muwaqqit.ProfileID
                    AND waqti.DynamicPrayerTimeProvider = 25);
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM ProfileLocations WHERE DynamicPrayerTimeProvider = 25;");

        migrationBuilder.DropTable(
            name: "WaqtiPrayerTimes");
    }
}
