using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UIApplication.Migrations
{
    /// <inheritdoc />
    public partial class InitialSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Content",
                table: "UserPrompts",
                newName: "Symbol");

            migrationBuilder.AddColumn<string>(
                name: "Exchange",
                table: "UserPrompts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Prompt",
                table: "UserPrompts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Exchange",
                table: "UserPrompts");

            migrationBuilder.DropColumn(
                name: "Prompt",
                table: "UserPrompts");

            migrationBuilder.RenameColumn(
                name: "Symbol",
                table: "UserPrompts",
                newName: "Content");
        }
    }
}
