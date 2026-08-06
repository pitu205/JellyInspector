using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JellyInspector.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialAuditSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Folder",
                table: "Series");

            migrationBuilder.RenameColumn(
                name: "LastScan",
                table: "Series",
                newName: "JellyfinId");

            migrationBuilder.RenameColumn(
                name: "FullPath",
                table: "Episodes",
                newName: "VideoCodec");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "Episodes",
                newName: "Runtime");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Series",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "ProductionYear",
                table: "Series",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "SeriesId",
                table: "Seasons",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Seasons",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<string>(
                name: "JellyfinId",
                table: "Seasons",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Seasons",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "SeasonId",
                table: "Episodes",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Episodes",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<string>(
                name: "AudioCodec",
                table: "Episodes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "Bitrate",
                table: "Episodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "Episodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "HasDolbyVision",
                table: "Episodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasHdr",
                table: "Episodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JellyfinId",
                table: "Episodes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Episodes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Resolution",
                table: "Episodes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Series_JellyfinId",
                table: "Series",
                column: "JellyfinId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_JellyfinId",
                table: "Seasons",
                column: "JellyfinId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_JellyfinId",
                table: "Episodes",
                column: "JellyfinId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Series_JellyfinId",
                table: "Series");

            migrationBuilder.DropIndex(
                name: "IX_Seasons_JellyfinId",
                table: "Seasons");

            migrationBuilder.DropIndex(
                name: "IX_Episodes_JellyfinId",
                table: "Episodes");

            migrationBuilder.DropColumn(
                name: "ProductionYear",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "JellyfinId",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "AudioCodec",
                table: "Episodes");

            migrationBuilder.DropColumn(
                name: "Bitrate",
                table: "Episodes");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Episodes");

            migrationBuilder.DropColumn(
                name: "HasDolbyVision",
                table: "Episodes");

            migrationBuilder.DropColumn(
                name: "HasHdr",
                table: "Episodes");

            migrationBuilder.DropColumn(
                name: "JellyfinId",
                table: "Episodes");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Episodes");

            migrationBuilder.DropColumn(
                name: "Resolution",
                table: "Episodes");

            migrationBuilder.RenameColumn(
                name: "JellyfinId",
                table: "Series",
                newName: "LastScan");

            migrationBuilder.RenameColumn(
                name: "VideoCodec",
                table: "Episodes",
                newName: "FullPath");

            migrationBuilder.RenameColumn(
                name: "Runtime",
                table: "Episodes",
                newName: "FileName");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Series",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<string>(
                name: "Folder",
                table: "Series",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "SeriesId",
                table: "Seasons",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Seasons",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<int>(
                name: "SeasonId",
                table: "Episodes",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Episodes",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Sqlite:Autoincrement", true);
        }
    }
}
