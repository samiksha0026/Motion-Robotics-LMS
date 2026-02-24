using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.Models;
using MotionRobotics.LMS.API.Services;

namespace MotionRobotics.LMS.Tests.Services
{
    public class StudentServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly StudentService _sut;

        public StudentServiceTests()
        {
            // Setup in-memory database for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _sut = new StudentService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetStudentIdByUserIdAsync Tests

        [Fact]
        public async Task GetStudentIdByUserIdAsync_WithValidUserId_ReturnsStudentId()
        {
            // Arrange
            var student = CreateTestStudent(1, "user-123");
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetStudentIdByUserIdAsync("user-123");

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public async Task GetStudentIdByUserIdAsync_WithInvalidUserId_ReturnsNull()
        {
            // Act
            var result = await _sut.GetStudentIdByUserIdAsync("nonexistent-user");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetDashboardAsync Tests

        [Fact]
        public async Task GetDashboardAsync_WithValidStudentId_ReturnsDashboard()
        {
            // Arrange
            await SeedBasicTestData();

            // Act
            var result = await _sut.GetDashboardAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.StudentId.Should().Be(1);
            result.FullName.Should().Be("Test Student");
            result.SchoolName.Should().Be("Test School");
            result.ClassName.Should().Be("Class 6");
        }

        [Fact]
        public async Task GetDashboardAsync_WithInvalidStudentId_ReturnsNull()
        {
            // Act
            var result = await _sut.GetDashboardAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetDashboardAsync_CalculatesProgressCorrectly()
        {
            // Arrange
            await SeedTestDataWithProgress();

            // Act
            var result = await _sut.GetDashboardAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Progress.TotalExperiments.Should().Be(3);
            result.Progress.CompletedExperiments.Should().Be(2);
            result.Progress.ApprovedExperiments.Should().Be(1);
            result.Progress.PendingApproval.Should().Be(1);
        }

        [Fact]
        public async Task GetDashboardAsync_ShowsCurrentLevel_WhenMappingExists()
        {
            // Arrange
            await SeedBasicTestData();

            // Act
            var result = await _sut.GetDashboardAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.CurrentLevel.Should().NotBeNull();
            result.CurrentLevel!.LevelNumber.Should().Be(1);
            result.CurrentLevel.LevelName.Should().Be("Beginner");
        }

        [Fact]
        public async Task GetDashboardAsync_ShowsNextExperiment_WhenAvailable()
        {
            // Arrange
            await SeedBasicTestData();

            // Act
            var result = await _sut.GetDashboardAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.NextExperiment.Should().NotBeNull();
            result.NextExperiment!.SequenceOrder.Should().Be(1);
        }

        #endregion

        #region GetProfileAsync Tests

        [Fact]
        public async Task GetProfileAsync_WithValidStudentId_ReturnsProfile()
        {
            // Arrange
            await SeedBasicTestData();

            // Act
            var result = await _sut.GetProfileAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.StudentId.Should().Be(1);
            result.FullName.Should().Be("Test Student");
            result.Email.Should().Be("student@test.com");
            result.RollNo.Should().Be("R001");
        }

        [Fact]
        public async Task GetProfileAsync_WithInvalidStudentId_ReturnsNull()
        {
            // Act
            var result = await _sut.GetProfileAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProfileAsync_IncludesSchoolAndClassInfo()
        {
            // Arrange
            await SeedBasicTestData();

            // Act
            var result = await _sut.GetProfileAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.SchoolName.Should().Be("Test School");
            result.ClassName.Should().Be("Class 6");
        }

        #endregion

        #region GetExperimentsAsync Tests

        [Fact]
        public async Task GetExperimentsAsync_WithValidStudentId_ReturnsExperiments()
        {
            // Arrange
            await SeedBasicTestData();

            // Act
            var result = await _sut.GetExperimentsAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Experiments.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetExperimentsAsync_WithInvalidStudentId_ReturnsNull()
        {
            // Act
            var result = await _sut.GetExperimentsAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetExperimentsAsync_ShowsCorrectSequenceOrder()
        {
            // Arrange
            await SeedBasicTestData();

            // Act
            var result = await _sut.GetExperimentsAsync(1);

            // Assert
            result.Should().NotBeNull();
            var sequences = result!.Experiments.Select(e => e.SequenceOrder).ToList();
            sequences.Should().BeInAscendingOrder();
        }

        #endregion

        #region GetCertificatesAsync Tests

        [Fact]
        public async Task GetCertificatesAsync_WithCertificates_ReturnsCertificates()
        {
            // Arrange
            await SeedTestDataWithCertificates();

            // Act
            var result = await _sut.GetCertificatesAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].LevelName.Should().Be("Beginner");
        }

        [Fact]
        public async Task GetCertificatesAsync_WithNoCertificates_ReturnsEmptyList()
        {
            // Arrange
            await SeedBasicTestData();

            // Act
            var result = await _sut.GetCertificatesAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region Helper Methods

        private Student CreateTestStudent(int id, string userId)
        {
            return new Student
            {
                Id = id,
                UserId = userId,
                Email = "student@test.com",
                FullName = "Test Student",
                RollNo = "R001",
                SchoolId = 1,
                ClassId = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        private async Task SeedBasicTestData()
        {
            // Add school
            var school = new School
            {
                Id = 1,
                SchoolName = "Test School",
                SchoolCode = "TS001",
                Address = "Test Address",
                City = "Test City",
                State = "Test State",
                Pincode = "123456",
                ContactEmail = "school@test.com",
                ContactPhone = "+91-9876543210"
            };
            _context.Schools.Add(school);

            // Add class
            var @class = new Class
            {
                Id = 1,
                ClassName = "Class 6",
                SchoolId = 1
            };
            _context.Classes.Add(@class);

            // Add academic year
            var academicYear = new AcademicYear
            {
                Id = 1,
                YearName = "2025-2026",
                IsCurrent = true,
                StartDate = new DateTime(2025, 4, 1),
                EndDate = new DateTime(2026, 3, 31)
            };
            _context.AcademicYears.Add(academicYear);

            // Add robotics level
            var level = new RoboticsLevel
            {
                Id = 1,
                LevelNumber = 1,
                Name = "Beginner",
                Description = "Level 1 - Beginner"
            };
            _context.RoboticsLevels.Add(level);

            // Add experiments
            _context.Experiments.AddRange(
                new Experiment { Id = 1, RoboticsLevelId = 1, Title = "Experiment 1", SequenceOrder = 1, EstimatedMinutes = 30 },
                new Experiment { Id = 2, RoboticsLevelId = 1, Title = "Experiment 2", SequenceOrder = 2, EstimatedMinutes = 45 },
                new Experiment { Id = 3, RoboticsLevelId = 1, Title = "Experiment 3", SequenceOrder = 3, EstimatedMinutes = 60 }
            );

            // Add level mapping
            var mapping = new SchoolLevelMapping
            {
                Id = 1,
                SchoolId = 1,
                ClassId = 1,
                RoboticsLevelId = 1,
                AcademicYearId = 1
            };
            _context.SchoolLevelMappings.Add(mapping);

            // Add class experiment unlocks (required for GetExperimentsAsync)
            _context.ClassExperimentUnlocks.AddRange(
                new ClassExperimentUnlock { Id = 1, ClassId = 1, ExperimentId = 1, UnlockedAt = DateTime.UtcNow },
                new ClassExperimentUnlock { Id = 2, ClassId = 1, ExperimentId = 2, UnlockedAt = DateTime.UtcNow },
                new ClassExperimentUnlock { Id = 3, ClassId = 1, ExperimentId = 3, UnlockedAt = DateTime.UtcNow }
            );

            // Add student
            var student = CreateTestStudent(1, "user-123");
            _context.Students.Add(student);

            await _context.SaveChangesAsync();
        }

        private async Task SeedTestDataWithProgress()
        {
            await SeedBasicTestData();

            // Add progress records
            _context.StudentProgress.AddRange(
                new StudentProgress
                {
                    Id = 1,
                    StudentId = 1,
                    ExperimentId = 1,
                    Completed = true,
                    IsApprovedByTeacher = true,
                    CompletedAt = DateTime.UtcNow.AddDays(-5)
                },
                new StudentProgress
                {
                    Id = 2,
                    StudentId = 1,
                    ExperimentId = 2,
                    Completed = true,
                    IsApprovedByTeacher = false,
                    CompletedAt = DateTime.UtcNow.AddDays(-1)
                }
            );

            await _context.SaveChangesAsync();
        }

        private async Task SeedTestDataWithCertificates()
        {
            await SeedBasicTestData();

            // Add certificate
            var certificate = new Certificate
            {
                Id = 1,
                StudentId = 1,
                SchoolId = 1,
                RoboticsLevelId = 1,
                AcademicYearId = 1,
                CertificateNumber = "CERT-001",
                IssuedDate = DateTime.UtcNow.AddDays(-10),
                CertificateFileUrl = "/certificates/cert-001.pdf",
                StudentName = "Test Student",
                StudentEmail = "student@test.com",
                SchoolName = "Test School",
                LevelName = "Beginner",
                LevelNumber = 1,
                AcademicYearName = "2025-2026"
            };
            _context.Certificates.Add(certificate);

            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
