using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotionRobotics.LMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSyllabusToRoboticsLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SyllabusUrl",
                table: "RoboticsLevels",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SyllabusUrl",
                table: "RoboticsLevels");
        }
    }
}
