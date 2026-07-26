using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexHire.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Day2_Onboarding_Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreditBalance",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreditResetDate",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "profile",
                table: "Users",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditBalance",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreditResetDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "profile",
                table: "Users");
        }
    }
}
