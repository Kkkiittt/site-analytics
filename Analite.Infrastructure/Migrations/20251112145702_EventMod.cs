using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EventMod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Handled",
                table: "Events",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Handled",
                table: "Events");
        }
    }
}
