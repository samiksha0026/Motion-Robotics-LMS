using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotionRobotics.LMS.API.Migrations
{
    /// <inheritdoc />
    public partial class RoboticsLevelSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProgramLevel",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ProgramName",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "ProgramName",
                table: "Experiments");

            migrationBuilder.DropColumn(
                name: "ProgramName",
                table: "Exams");

            migrationBuilder.RenameColumn(
                name: "Level",
                table: "Experiments",
                newName: "SequenceOrder");

            migrationBuilder.RenameColumn(
                name: "DemoVideo",
                table: "Experiments",
                newName: "Procedure");

            migrationBuilder.RenameColumn(
                name: "TimerMinutes",
                table: "Exams",
                newName: "TotalQuestions");

            migrationBuilder.RenameColumn(
                name: "Level",
                table: "Exams",
                newName: "RoboticsLevelId");

            migrationBuilder.RenameColumn(
                name: "IsUnlocked",
                table: "Exams",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "ProgramName",
                table: "Certificates",
                newName: "StudentName");

            migrationBuilder.RenameColumn(
                name: "Level",
                table: "Certificates",
                newName: "StudentId");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Teachers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Students",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "CurrentAcademicYearId",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ParentName",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentPhone",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicYearId",
                table: "StudentProgress",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "StudentProgress",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByTeacherId",
                table: "StudentProgress",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "StudentProgress",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsApprovedByTeacher",
                table: "StudentProgress",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "StudentProgress",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SubmissionImageUrl",
                table: "StudentProgress",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmissionNotes",
                table: "StudentProgress",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeacherRemarks",
                table: "StudentProgress",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "StudentProgress",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SchoolCode",
                table: "Schools",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Schools",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Schools",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Pincode",
                table: "Schools",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SchoolAdminUserId",
                table: "Schools",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Schools",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CircuitDiagram",
                table: "Experiments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Experiments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DemoVideoUrl",
                table: "Experiments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Experiments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedMinutes",
                table: "Experiments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Experiments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RoboticsLevelId",
                table: "Experiments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Experiments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Exams",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Exams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PassingPercentage",
                table: "Exams",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "QuestionsJson",
                table: "Exams",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMarks",
                table: "Exams",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Exams",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicYearId",
                table: "Certificates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AcademicYearName",
                table: "Certificates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CertificateFileUrl",
                table: "Certificates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CertificateNumber",
                table: "Certificates",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Certificates",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "ExamScore",
                table: "Certificates",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "LevelName",
                table: "Certificates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LevelNumber",
                table: "Certificates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PassingScore",
                table: "Certificates",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "RoboticsLevelId",
                table: "Certificates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SchoolId",
                table: "Certificates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SchoolLogoUrl",
                table: "Certificates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolName",
                table: "Certificates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AcademicYears",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    YearName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicYears", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoboticsLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LevelNumber = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoboticsLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExamResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    ScoreObtained = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TotalMarks = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IsPassed = table.Column<bool>(type: "bit", nullable: false),
                    AnswersJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeTakenSeconds = table.Column<int>(type: "int", nullable: false),
                    CertificateId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamResults_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamResults_Certificates_CertificateId",
                        column: x => x.CertificateId,
                        principalTable: "Certificates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ExamResults_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamResults_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchoolLevelMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    RoboticsLevelId = table.Column<int>(type: "int", nullable: false),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolLevelMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolLevelMappings_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolLevelMappings_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolLevelMappings_RoboticsLevels_RoboticsLevelId",
                        column: x => x.RoboticsLevelId,
                        principalTable: "RoboticsLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolLevelMappings_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_Email",
                table: "Teachers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_CurrentAcademicYearId",
                table: "Students",
                column: "CurrentAcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Email",
                table: "Students",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgress_AcademicYearId",
                table: "StudentProgress",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgress_ApprovedByTeacherId",
                table: "StudentProgress",
                column: "ApprovedByTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgress_ExperimentId",
                table: "StudentProgress",
                column: "ExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgress_StudentId_ExperimentId_AcademicYearId",
                table: "StudentProgress",
                columns: new[] { "StudentId", "ExperimentId", "AcademicYearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schools_SchoolAdminUserId",
                table: "Schools",
                column: "SchoolAdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_SchoolCode",
                table: "Schools",
                column: "SchoolCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Experiments_RoboticsLevelId_SequenceOrder",
                table: "Experiments",
                columns: new[] { "RoboticsLevelId", "SequenceOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exams_RoboticsLevelId",
                table: "Exams",
                column: "RoboticsLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_AcademicYearId",
                table: "Certificates",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CertificateNumber",
                table: "Certificates",
                column: "CertificateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_RoboticsLevelId",
                table: "Certificates",
                column: "RoboticsLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_SchoolId",
                table: "Certificates",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_StudentId",
                table: "Certificates",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_AcademicYearId",
                table: "ExamResults",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_CertificateId",
                table: "ExamResults",
                column: "CertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_ExamId",
                table: "ExamResults",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_StudentId",
                table: "ExamResults",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_RoboticsLevels_LevelNumber",
                table: "RoboticsLevels",
                column: "LevelNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolLevelMappings_AcademicYearId",
                table: "SchoolLevelMappings",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolLevelMappings_ClassId",
                table: "SchoolLevelMappings",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolLevelMappings_RoboticsLevelId",
                table: "SchoolLevelMappings",
                column: "RoboticsLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolLevelMappings_SchoolId_ClassId_AcademicYearId",
                table: "SchoolLevelMappings",
                columns: new[] { "SchoolId", "ClassId", "AcademicYearId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_AcademicYears_AcademicYearId",
                table: "Certificates",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_RoboticsLevels_RoboticsLevelId",
                table: "Certificates",
                column: "RoboticsLevelId",
                principalTable: "RoboticsLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Schools_SchoolId",
                table: "Certificates",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Students_StudentId",
                table: "Certificates",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_RoboticsLevels_RoboticsLevelId",
                table: "Exams",
                column: "RoboticsLevelId",
                principalTable: "RoboticsLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Experiments_RoboticsLevels_RoboticsLevelId",
                table: "Experiments",
                column: "RoboticsLevelId",
                principalTable: "RoboticsLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_AspNetUsers_SchoolAdminUserId",
                table: "Schools",
                column: "SchoolAdminUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProgress_AcademicYears_AcademicYearId",
                table: "StudentProgress",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProgress_Experiments_ExperimentId",
                table: "StudentProgress",
                column: "ExperimentId",
                principalTable: "Experiments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProgress_Students_StudentId",
                table: "StudentProgress",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProgress_Teachers_ApprovedByTeacherId",
                table: "StudentProgress",
                column: "ApprovedByTeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_AcademicYears_CurrentAcademicYearId",
                table: "Students",
                column: "CurrentAcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_AcademicYears_AcademicYearId",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_RoboticsLevels_RoboticsLevelId",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Schools_SchoolId",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Students_StudentId",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_Exams_RoboticsLevels_RoboticsLevelId",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_Experiments_RoboticsLevels_RoboticsLevelId",
                table: "Experiments");

            migrationBuilder.DropForeignKey(
                name: "FK_Schools_AspNetUsers_SchoolAdminUserId",
                table: "Schools");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentProgress_AcademicYears_AcademicYearId",
                table: "StudentProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentProgress_Experiments_ExperimentId",
                table: "StudentProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentProgress_Students_StudentId",
                table: "StudentProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentProgress_Teachers_ApprovedByTeacherId",
                table: "StudentProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_AcademicYears_CurrentAcademicYearId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "ExamResults");

            migrationBuilder.DropTable(
                name: "SchoolLevelMappings");

            migrationBuilder.DropTable(
                name: "AcademicYears");

            migrationBuilder.DropTable(
                name: "RoboticsLevels");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_Email",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Students_CurrentAcademicYearId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_Email",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_StudentProgress_AcademicYearId",
                table: "StudentProgress");

            migrationBuilder.DropIndex(
                name: "IX_StudentProgress_ApprovedByTeacherId",
                table: "StudentProgress");

            migrationBuilder.DropIndex(
                name: "IX_StudentProgress_ExperimentId",
                table: "StudentProgress");

            migrationBuilder.DropIndex(
                name: "IX_StudentProgress_StudentId_ExperimentId_AcademicYearId",
                table: "StudentProgress");

            migrationBuilder.DropIndex(
                name: "IX_Schools_SchoolAdminUserId",
                table: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_Schools_SchoolCode",
                table: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_Experiments_RoboticsLevelId_SequenceOrder",
                table: "Experiments");

            migrationBuilder.DropIndex(
                name: "IX_Exams_RoboticsLevelId",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_AcademicYearId",
                table: "Certificates");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_CertificateNumber",
                table: "Certificates");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_RoboticsLevelId",
                table: "Certificates");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_SchoolId",
                table: "Certificates");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_StudentId",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "CurrentAcademicYearId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ParentName",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ParentPhone",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "StudentProgress");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "StudentProgress");

            migrationBuilder.DropColumn(
                name: "ApprovedByTeacherId",
                table: "StudentProgress");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "StudentProgress");

            migrationBuilder.DropColumn(
                name: "IsApprovedByTeacher",
                table: "StudentProgress");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "StudentProgress");

            migrationBuilder.DropColumn(
                name: "SubmissionImageUrl",
                table: "StudentProgress");

            migrationBuilder.DropColumn(
                name: "SubmissionNotes",
                table: "StudentProgress");

            migrationBuilder.DropColumn(
                name: "TeacherRemarks",
                table: "StudentProgress");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StudentProgress");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "Pincode",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "SchoolAdminUserId",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "CircuitDiagram",
                table: "Experiments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Experiments");

            migrationBuilder.DropColumn(
                name: "DemoVideoUrl",
                table: "Experiments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Experiments");

            migrationBuilder.DropColumn(
                name: "EstimatedMinutes",
                table: "Experiments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Experiments");

            migrationBuilder.DropColumn(
                name: "RoboticsLevelId",
                table: "Experiments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Experiments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "PassingPercentage",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "QuestionsJson",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "TotalMarks",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "AcademicYearName",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "CertificateFileUrl",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "CertificateNumber",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "ExamScore",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "LevelName",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "LevelNumber",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "PassingScore",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "RoboticsLevelId",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "SchoolLogoUrl",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "SchoolName",
                table: "Certificates");

            migrationBuilder.RenameColumn(
                name: "SequenceOrder",
                table: "Experiments",
                newName: "Level");

            migrationBuilder.RenameColumn(
                name: "Procedure",
                table: "Experiments",
                newName: "DemoVideo");

            migrationBuilder.RenameColumn(
                name: "TotalQuestions",
                table: "Exams",
                newName: "TimerMinutes");

            migrationBuilder.RenameColumn(
                name: "RoboticsLevelId",
                table: "Exams",
                newName: "Level");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Exams",
                newName: "IsUnlocked");

            migrationBuilder.RenameColumn(
                name: "StudentName",
                table: "Certificates",
                newName: "ProgramName");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Certificates",
                newName: "Level");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "ProgramLevel",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProgramName",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "SchoolCode",
                table: "Schools",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Schools",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgramName",
                table: "Experiments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProgramName",
                table: "Exams",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
