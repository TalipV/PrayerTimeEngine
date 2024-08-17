using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrayerTimeEngine.Core.Data.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlaceInfos_TimezoneInfo_TimezoneInfoID",
                table: "PlaceInfos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimezoneInfo",
                table: "TimezoneInfo");

            migrationBuilder.RenameTable(
                name: "TimezoneInfo",
                newName: "TimezoneInfos");

            migrationBuilder.AddColumn<string>(
                name: "InsertInstant",
                table: "TimezoneInfos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimezoneInfos",
                table: "TimezoneInfos",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PlaceInfos_TimezoneInfos_TimezoneInfoID",
                table: "PlaceInfos",
                column: "TimezoneInfoID",
                principalTable: "TimezoneInfos",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlaceInfos_TimezoneInfos_TimezoneInfoID",
                table: "PlaceInfos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimezoneInfos",
                table: "TimezoneInfos");

            migrationBuilder.DropColumn(
                name: "InsertInstant",
                table: "TimezoneInfos");

            migrationBuilder.RenameTable(
                name: "TimezoneInfos",
                newName: "TimezoneInfo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimezoneInfo",
                table: "TimezoneInfo",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PlaceInfos_TimezoneInfo_TimezoneInfoID",
                table: "PlaceInfos",
                column: "TimezoneInfoID",
                principalTable: "TimezoneInfo",
                principalColumn: "ID");
        }
    }
}
