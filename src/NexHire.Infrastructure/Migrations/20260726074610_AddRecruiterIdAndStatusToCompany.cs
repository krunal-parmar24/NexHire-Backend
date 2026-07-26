using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexHire.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruiterIdAndStatusToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RecruiterId",
                table: "Companies",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "VerificationStatus",
                table: "Companies",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecruiterId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "Companies");
        }
    }
}
