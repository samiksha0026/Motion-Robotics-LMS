using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotionRobotics.LMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddClassExperimentUnlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassExperimentUnlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    ExperimentId = table.Column<int>(type: "int", nullable: false),
                    UnlockedByTeacherId = table.Column<int>(type: "int", nullable: false),
                    UnlockedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassExperimentUnlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassExperimentUnlocks_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassExperimentUnlocks_Experiments_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "Experiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassExperimentUnlocks_Teachers_UnlockedByTeacherId",
                        column: x => x.UnlockedByTeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassExperimentUnlocks_ClassId_ExperimentId",
                table: "ClassExperimentUnlocks",
                columns: new[] { "ClassId", "ExperimentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassExperimentUnlocks_ExperimentId",
                table: "ClassExperimentUnlocks",
                column: "ExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassExperimentUnlocks_UnlockedByTeacherId",
                table: "ClassExperimentUnlocks",
                column: "UnlockedByTeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassExperimentUnlocks");
        }
    }
}
