using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MotionRobotics.LMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddLabPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LabArea",
                table: "Schools",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LabCapacity",
                table: "Schools",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabDescription",
                table: "Schools",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LabPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    Caption = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabPhotos_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabPhotos_SchoolId_DisplayOrder",
                table: "LabPhotos",
                columns: new[] { "SchoolId", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabPhotos");

            migrationBuilder.DropColumn(
                name: "LabArea",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "LabCapacity",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "LabDescription",
                table: "Schools");
        }
    }
}
