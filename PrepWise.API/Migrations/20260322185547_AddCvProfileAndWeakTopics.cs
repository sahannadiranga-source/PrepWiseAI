using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrepWise.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCvProfileAndWeakTopics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WeakTopics",
                table: "SessionAnalytics",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CvProfile",
                table: "InterviewSessions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeakTopics",
                table: "SessionAnalytics");

            migrationBuilder.DropColumn(
                name: "CvProfile",
                table: "InterviewSessions");
        }
    }
}
