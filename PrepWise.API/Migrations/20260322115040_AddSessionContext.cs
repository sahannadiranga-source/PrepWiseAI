using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrepWise.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalContext",
                table: "InterviewSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InterviewGoal",
                table: "InterviewSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "InterviewSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TargetRole",
                table: "InterviewSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalContext",
                table: "InterviewSessions");

            migrationBuilder.DropColumn(
                name: "InterviewGoal",
                table: "InterviewSessions");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "InterviewSessions");

            migrationBuilder.DropColumn(
                name: "TargetRole",
                table: "InterviewSessions");
        }
    }
}
