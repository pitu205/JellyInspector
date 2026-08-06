using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JellyInspector.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeriesMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageTag",
                table: "Series",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Overview",
                table: "Series",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageTag",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "Overview",
                table: "Series");
        }
    }
}
