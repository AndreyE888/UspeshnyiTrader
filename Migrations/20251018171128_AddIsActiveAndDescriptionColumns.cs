using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UspeshnyiTrader.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveAndDescriptionColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Payout",
                table: "Trades",
                newName: "Profit");

            migrationBuilder.RenameColumn(
                name: "OpenPrice",
                table: "Trades",
                newName: "EntryPrice");

            migrationBuilder.RenameColumn(
                name: "Direction",
                table: "Trades",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "ClosePrice",
                table: "Trades",
                newName: "ExitPrice");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CloseTime",
                table: "Trades",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "Trades",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Trades",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastPriceUpdate",
                table: "Instruments",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Instruments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Instruments",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Instruments");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Instruments");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Trades",
                newName: "Direction");

            migrationBuilder.RenameColumn(
                name: "Profit",
                table: "Trades",
                newName: "Payout");

            migrationBuilder.RenameColumn(
                name: "ExitPrice",
                table: "Trades",
                newName: "ClosePrice");

            migrationBuilder.RenameColumn(
                name: "EntryPrice",
                table: "Trades",
                newName: "OpenPrice");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CloseTime",
                table: "Trades",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastPriceUpdate",
                table: "Instruments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
