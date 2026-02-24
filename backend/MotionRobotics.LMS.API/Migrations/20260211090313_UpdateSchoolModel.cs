using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotionRobotics.LMS.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchoolModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Schools",
                newName: "SchoolCode");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Schools",
                newName: "PrincipalName");

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Schools",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Schools",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Schools",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Schools",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Schools");

            migrationBuilder.RenameColumn(
                name: "SchoolCode",
                table: "Schools",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "PrincipalName",
                table: "Schools",
                newName: "Email");
        }
    }
}
