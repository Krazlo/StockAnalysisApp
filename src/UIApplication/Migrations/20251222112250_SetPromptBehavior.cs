using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UIApplication.Migrations
{
    /// <inheritdoc />
    public partial class SetPromptBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockAnalyses_UserPrompts_UserPromptId",
                table: "StockAnalyses");

            migrationBuilder.AddForeignKey(
                name: "FK_StockAnalyses_UserPrompts_UserPromptId",
                table: "StockAnalyses",
                column: "UserPromptId",
                principalTable: "UserPrompts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockAnalyses_UserPrompts_UserPromptId",
                table: "StockAnalyses");

            migrationBuilder.AddForeignKey(
                name: "FK_StockAnalyses_UserPrompts_UserPromptId",
                table: "StockAnalyses",
                column: "UserPromptId",
                principalTable: "UserPrompts",
                principalColumn: "Id");
        }
    }
}
