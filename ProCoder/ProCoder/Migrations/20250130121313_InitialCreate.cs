using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProCoder.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Coder_CoderId",
                table: "ChatMessages");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "ChatMessages",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getutcdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Coder",
                table: "ChatMessages",
                column: "CoderId",
                principalTable: "Coder",
                principalColumn: "CoderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Coder",
                table: "ChatMessages");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "ChatMessages",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getutcdate())");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Coder_CoderId",
                table: "ChatMessages",
                column: "CoderId",
                principalTable: "Coder",
                principalColumn: "CoderId");
        }
    }
}
