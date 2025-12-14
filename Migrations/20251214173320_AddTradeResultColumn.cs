using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UspeshnyiTrader.Migrations
{
    /// <inheritdoc />
    public partial class AddTradeResultColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsWin",
                table: "Trades");

            migrationBuilder.AddColumn<string>(
                name: "Result",
                table: "Trades",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_Result",
                table: "Trades",
                column: "Result");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trades_Result",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "Result",
                table: "Trades");

            migrationBuilder.AddColumn<bool>(
                name: "IsWin",
                table: "Trades",
                type: "boolean",
                nullable: true);
        }
    }
}
