using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexHire.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResumeUrlToApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResumeUrl",
                table: "Applications",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_JobId_Status",
                table: "Applications",
                columns: new[] { "JobId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applications_JobId_Status",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "ResumeUrl",
                table: "Applications");
        }
    }
}
