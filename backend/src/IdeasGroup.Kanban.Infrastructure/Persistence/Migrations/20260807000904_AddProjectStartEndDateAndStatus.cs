using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeasGroup.Kanban.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectStartEndDateAndStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedEndDate",
                table: "Projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Projects",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Planned");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedEndDate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Projects");
        }
    }
}
