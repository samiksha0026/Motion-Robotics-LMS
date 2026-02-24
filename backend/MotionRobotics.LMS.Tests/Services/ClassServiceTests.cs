using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Models;
using MotionRobotics.LMS.API.Repositories.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.Tests.Services
{
    public class ClassServiceTests : IDisposable
    {
        private readonly Mock<IClassRepository> _classRepositoryMock;
        private readonly ApplicationDbContext _context;
        private readonly ClassService _sut;

        public ClassServiceTests()
        {
            _classRepositoryMock = new Mock<IClassRepository>();

            // Setup in-memory database for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _sut = new ClassService(_classRepositoryMock.Object, _context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetClassByIdAsync Tests

        [Fact]
        public async Task GetClassByIdAsync_WithValidId_ReturnsClass()
        {
            // Arrange
            var testClass = CreateTestClass(1, "Class 6", 1, "Delhi Public School");

            _classRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(testClass);

            // Act
            var result = await _sut.GetClassByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.ClassName.Should().Be("Class 6");
            result.SchoolId.Should().Be(1);
        }

        [Fact]
        public async Task GetClassByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            _classRepositoryMock.Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((Class?)null);

            // Act
            var result = await _sut.GetClassByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetAllClassesAsync Tests

        [Fact]
        public async Task GetAllClassesAsync_ReturnsAllClasses()
        {
            // Arrange
            var classes = new List<Class>
            {
                CreateTestClass(1, "Class 6", 1, "DPS"),
                CreateTestClass(2, "Class 7", 1, "DPS"),
                CreateTestClass(3, "Class 8", 2, "KV")
            };

            _classRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(classes);

            // Act
            var result = await _sut.GetAllClassesAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Select(c => c.ClassName).Should().Contain(new[] { "Class 6", "Class 7", "Class 8" });
        }

        [Fact]
        public async Task GetAllClassesAsync_WithNoClasses_ReturnsEmptyList()
        {
            // Arrange
            _classRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Class>());

            // Act
            var result = await _sut.GetAllClassesAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllClassesAsync_IncludesRoboticsLevelInfo_WhenMappingExists()
        {
            // Arrange
            var classes = new List<Class>
            {
                CreateTestClass(1, "Class 6", 1, "DPS")
            };

            _classRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(classes);

            // Add academic year and level mapping to in-memory database
            var academicYear = new AcademicYear
            {
                Id = 1,
                YearName = "2025-2026",
                IsCurrent = true,
                StartDate = new DateTime(2025, 4, 1),
                EndDate = new DateTime(2026, 3, 31)
            };
            _context.AcademicYears.Add(academicYear);

            var roboticsLevel = new RoboticsLevel
            {
                Id = 1,
                LevelNumber = 1,
                Name = "Beginner",
                Description = "Level 1"
            };
            _context.RoboticsLevels.Add(roboticsLevel);

            var levelMapping = new SchoolLevelMapping
            {
                Id = 1,
                SchoolId = 1,
                ClassId = 1,
                RoboticsLevelId = 1,
                AcademicYearId = 1,
                RoboticsLevel = roboticsLevel
            };
            _context.SchoolLevelMappings.Add(levelMapping);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetAllClassesAsync();

            // Assert
            result.Should().HaveCount(1);
            result[0].RoboticsLevelId.Should().Be(1);
            result[0].RoboticsLevelName.Should().Be("Beginner");
            result[0].RoboticsLevelNumber.Should().Be(1);
        }

        #endregion

        #region GetClassesBySchoolAsync Tests

        [Fact]
        public async Task GetClassesBySchoolAsync_ReturnsClassesForSchool()
        {
            // Arrange
            var schoolId = 1;
            var classes = new List<Class>
            {
                CreateTestClass(1, "Class 6", schoolId, "DPS"),
                CreateTestClass(2, "Class 7", schoolId, "DPS")
            };

            _classRepositoryMock.Setup(x => x.GetBySchoolIdAsync(schoolId))
                .ReturnsAsync(classes);

            // Act
            var result = await _sut.GetClassesBySchoolAsync(schoolId);

            // Assert
            result.Should().HaveCount(2);
            result.All(c => c.SchoolId == schoolId).Should().BeTrue();
        }

        [Fact]
        public async Task GetClassesBySchoolAsync_WithNoClasses_ReturnsEmptyList()
        {
            // Arrange
            _classRepositoryMock.Setup(x => x.GetBySchoolIdAsync(999))
                .ReturnsAsync(new List<Class>());

            // Act
            var result = await _sut.GetClassesBySchoolAsync(999);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region CreateClassAsync Tests

        [Fact]
        public async Task CreateClassAsync_WithValidData_CreatesClass()
        {
            // Arrange
            var dto = new ClassCreateDto
            {
                ClassName = "Class 6",
                SchoolId = 1
            };

            _classRepositoryMock.Setup(x => x.ExistsByNameAndSchoolAsync(dto.ClassName, dto.SchoolId))
                .ReturnsAsync(false);

            _classRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Class>()))
                .ReturnsAsync((Class c) =>
                {
                    c.Id = 1;
                    c.School = new School { Id = 1, SchoolName = "DPS" };
                    return c;
                });

            // Act
            var result = await _sut.CreateClassAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.ClassName.Should().Be("Class 6");
            result.SchoolId.Should().Be(1);
        }

        [Fact]
        public async Task CreateClassAsync_WithDuplicateName_ThrowsException()
        {
            // Arrange
            var dto = new ClassCreateDto
            {
                ClassName = "Existing Class",
                SchoolId = 1
            };

            _classRepositoryMock.Setup(x => x.ExistsByNameAndSchoolAsync(dto.ClassName, dto.SchoolId))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.CreateClassAsync(dto));
        }

        [Fact]
        public async Task CreateClassAsync_WithRoboticsLevel_CreatesLevelMapping()
        {
            // Arrange
            var dto = new ClassCreateDto
            {
                ClassName = "Class 6",
                SchoolId = 1,
                RoboticsLevelId = 1
            };

            // Add academic year to in-memory database
            var academicYear = new AcademicYear
            {
                Id = 1,
                YearName = "2025-2026",
                IsCurrent = true,
                StartDate = new DateTime(2025, 4, 1),
                EndDate = new DateTime(2026, 3, 31)
            };
            _context.AcademicYears.Add(academicYear);

            var roboticsLevel = new RoboticsLevel
            {
                Id = 1,
                LevelNumber = 1,
                Name = "Beginner"
            };
            _context.RoboticsLevels.Add(roboticsLevel);
            await _context.SaveChangesAsync();

            _classRepositoryMock.Setup(x => x.ExistsByNameAndSchoolAsync(dto.ClassName, dto.SchoolId))
                .ReturnsAsync(false);

            _classRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Class>()))
                .ReturnsAsync((Class c) =>
                {
                    c.Id = 1;
                    c.School = new School { Id = 1, SchoolName = "DPS" };
                    return c;
                });

            // Act
            var result = await _sut.CreateClassAsync(dto);

            // Assert
            result.Should().NotBeNull();

            // Verify level mapping was created
            var mapping = await _context.SchoolLevelMappings.FirstOrDefaultAsync(m => m.ClassId == 1);
            mapping.Should().NotBeNull();
            mapping!.RoboticsLevelId.Should().Be(1);
        }

        #endregion

        #region Helper Methods

        private static Class CreateTestClass(int id, string className, int schoolId, string schoolName)
        {
            return new Class
            {
                Id = id,
                ClassName = className,
                SchoolId = schoolId,
                School = new School
                {
                    Id = schoolId,
                    SchoolName = schoolName,
                    SchoolCode = schoolName.ToUpper().Replace(" ", ""),
                    Address = "Test Address",
                    City = "Test City",
                    State = "Test State",
                    Pincode = "123456",
                    ContactEmail = "test@school.com",
                    ContactPhone = "+91-9876543210"
                },
                CreatedAt = DateTime.UtcNow
            };
        }

        #endregion
    }
}
