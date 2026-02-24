using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotionRobotics.LMS.API.Migrations
{
    /// <inheritdoc />
    public partial class TeacherUserIdAndStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Teachers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Teachers");
        }
    }
}
