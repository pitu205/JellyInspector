using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JellyInspector.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddScanSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ScanSessionId",
                table: "ScanIssues",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ScanSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FinishedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Series = table.Column<int>(type: "INTEGER", nullable: false),
                    Seasons = table.Column<int>(type: "INTEGER", nullable: false),
                    Episodes = table.Column<int>(type: "INTEGER", nullable: false),
                    IssueCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScanIssues_ScanSessionId",
                table: "ScanIssues",
                column: "ScanSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScanIssues_ScanSessions_ScanSessionId",
                table: "ScanIssues",
                column: "ScanSessionId",
                principalTable: "ScanSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScanIssues_ScanSessions_ScanSessionId",
                table: "ScanIssues");

            migrationBuilder.DropTable(
                name: "ScanSessions");

            migrationBuilder.DropIndex(
                name: "IX_ScanIssues_ScanSessionId",
                table: "ScanIssues");

            migrationBuilder.DropColumn(
                name: "ScanSessionId",
                table: "ScanIssues");
        }
    }
}
