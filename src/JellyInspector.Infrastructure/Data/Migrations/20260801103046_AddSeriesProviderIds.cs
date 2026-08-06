using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JellyInspector.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeriesProviderIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImdbId",
                table: "Series",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TmdbId",
                table: "Series",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TvdbId",
                table: "Series",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImdbId",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "TmdbId",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "TvdbId",
                table: "Series");
        }
    }
}
