using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Moq;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Models;
using MotionRobotics.LMS.API.Repositories.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.Tests.Services
{
    public class SchoolServiceTests
    {
        private readonly Mock<ISchoolRepository> _schoolRepositoryMock;
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<IWebHostEnvironment> _environmentMock;
        private readonly SchoolService _sut;

        public SchoolServiceTests()
        {
            _schoolRepositoryMock = new Mock<ISchoolRepository>();

            // Setup UserManager mock
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            // Setup RoleManager mock
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object, null!, null!, null!, null!);

            _environmentMock = new Mock<IWebHostEnvironment>();
            _environmentMock.Setup(e => e.WebRootPath).Returns("wwwroot");

            _sut = new SchoolService(
                _schoolRepositoryMock.Object,
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _environmentMock.Object);
        }

        #region GetSchoolByIdAsync Tests

        [Fact]
        public async Task GetSchoolByIdAsync_WithValidId_ReturnsSchool()
        {
            // Arrange
            var school = CreateTestSchool(1, "Delhi Public School", "DPS001");

            _schoolRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(school);

            // Act
            var result = await _sut.GetSchoolByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.SchoolName.Should().Be("Delhi Public School");
            result.SchoolCode.Should().Be("DPS001");
        }

        [Fact]
        public async Task GetSchoolByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            _schoolRepositoryMock.Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((School?)null);

            // Act
            var result = await _sut.GetSchoolByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetAllSchoolsAsync Tests

        [Fact]
        public async Task GetAllSchoolsAsync_ReturnsAllSchools()
        {
            // Arrange
            var schools = new List<School>
            {
                CreateTestSchool(1, "Delhi Public School", "DPS001"),
                CreateTestSchool(2, "Kendriya Vidyalaya", "KV002"),
                CreateTestSchool(3, "DAV School", "DAV003")
            };

            _schoolRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(schools);

            // Act
            var result = await _sut.GetAllSchoolsAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Select(s => s.SchoolName).Should().BeEquivalentTo(
                new[] { "Delhi Public School", "Kendriya Vidyalaya", "DAV School" });
        }

        [Fact]
        public async Task GetAllSchoolsAsync_WithNoSchools_ReturnsEmptyList()
        {
            // Arrange
            _schoolRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<School>());

            // Act
            var result = await _sut.GetAllSchoolsAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region CreateSchoolAsync Tests

        [Fact]
        public async Task CreateSchoolAsync_WithValidData_CreatesSchool()
        {
            // Arrange
            var dto = CreateTestSchoolDto("New School", "NS001");

            _schoolRepositoryMock.Setup(x => x.ExistsByNameAsync(dto.SchoolName))
                .ReturnsAsync(false);

            _schoolRepositoryMock.Setup(x => x.ExistsByCodeAsync(dto.SchoolCode))
                .ReturnsAsync(false);

            _roleManagerMock.Setup(x => x.RoleExistsAsync("SchoolAdmin"))
                .ReturnsAsync(true);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), "SchoolAdmin"))
                .ReturnsAsync(IdentityResult.Success);

            _schoolRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<School>()))
                .ReturnsAsync((School s) =>
                {
                    s.Id = 1;
                    return s;
                });

            // Act
            var result = await _sut.CreateSchoolAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.SchoolName.Should().Be("New School");
            result.SchoolCode.Should().Be("NS001");
            result.LoginUsername.Should().NotBeNullOrEmpty();
            result.LoginPassword.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CreateSchoolAsync_WithDuplicateName_ThrowsException()
        {
            // Arrange
            var dto = CreateTestSchoolDto("Existing School", "ES001");

            _schoolRepositoryMock.Setup(x => x.ExistsByNameAsync(dto.SchoolName))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.CreateSchoolAsync(dto));
        }

        [Fact]
        public async Task CreateSchoolAsync_WithDuplicateCode_ThrowsException()
        {
            // Arrange
            var dto = CreateTestSchoolDto("New School", "EXISTINGCODE");

            _schoolRepositoryMock.Setup(x => x.ExistsByNameAsync(dto.SchoolName))
                .ReturnsAsync(false);

            _schoolRepositoryMock.Setup(x => x.ExistsByCodeAsync(dto.SchoolCode))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.CreateSchoolAsync(dto));
        }

        [Fact]
        public async Task CreateSchoolAsync_CreatesSchoolAdminRole_IfNotExists()
        {
            // Arrange
            var dto = CreateTestSchoolDto("New School", "NS001");

            _schoolRepositoryMock.Setup(x => x.ExistsByNameAsync(dto.SchoolName))
                .ReturnsAsync(false);

            _schoolRepositoryMock.Setup(x => x.ExistsByCodeAsync(dto.SchoolCode))
                .ReturnsAsync(false);

            _roleManagerMock.Setup(x => x.RoleExistsAsync("SchoolAdmin"))
                .ReturnsAsync(false);

            _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), "SchoolAdmin"))
                .ReturnsAsync(IdentityResult.Success);

            _schoolRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<School>()))
                .ReturnsAsync((School s) => { s.Id = 1; return s; });

            // Act
            await _sut.CreateSchoolAsync(dto);

            // Assert
            _roleManagerMock.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == "SchoolAdmin")), Times.Once);
        }

        #endregion

        #region UpdateSchoolAsync Tests

        [Fact]
        public async Task UpdateSchoolAsync_WithValidData_UpdatesSchool()
        {
            // Arrange
            var existingSchool = CreateTestSchool(1, "Old Name", "OLD001");
            var dto = CreateTestSchoolDto("Updated Name", "OLD001");

            _schoolRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(existingSchool);

            _schoolRepositoryMock.Setup(x => x.ExistsByNameAsync(dto.SchoolName))
                .ReturnsAsync(false);

            _schoolRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<School>()))
                .ReturnsAsync((School s) => s);

            // Act
            var result = await _sut.UpdateSchoolAsync(1, dto);

            // Assert
            result.Should().NotBeNull();
            result.SchoolName.Should().Be("Updated Name");
        }

        [Fact]
        public async Task UpdateSchoolAsync_WithNonExistentId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = CreateTestSchoolDto("Name", "CODE");

            _schoolRepositoryMock.Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((School?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _sut.UpdateSchoolAsync(999, dto));
        }

        [Fact]
        public async Task UpdateSchoolAsync_WithDuplicateName_ThrowsException()
        {
            // Arrange
            var existingSchool = CreateTestSchool(1, "Original Name", "ORG001");
            var dto = CreateTestSchoolDto("Duplicate Name", "ORG001");

            _schoolRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(existingSchool);

            _schoolRepositoryMock.Setup(x => x.ExistsByNameAsync("Duplicate Name"))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.UpdateSchoolAsync(1, dto));
        }

        #endregion

        #region DeleteSchoolAsync Tests

        [Fact]
        public async Task DeleteSchoolAsync_WithValidId_DeletesSchoolAndUser()
        {
            // Arrange
            var school = CreateTestSchool(1, "School To Delete", "DEL001");
            school.SchoolAdminUserId = "user-id-123";

            var user = new IdentityUser { Id = "user-id-123" };

            _schoolRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(school);

            _userManagerMock.Setup(x => x.FindByIdAsync("user-id-123"))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            _schoolRepositoryMock.Setup(x => x.DeleteAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteSchoolAsync(1);

            // Assert
            result.Should().BeTrue();
            _userManagerMock.Verify(x => x.DeleteAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeleteSchoolAsync_WithNonExistentSchool_ReturnsFalse()
        {
            // Arrange
            _schoolRepositoryMock.Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((School?)null);

            _schoolRepositoryMock.Setup(x => x.DeleteAsync(999))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.DeleteSchoolAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Helper Methods

        private static School CreateTestSchool(int id, string name, string code)
        {
            return new School
            {
                Id = id,
                SchoolName = name,
                SchoolCode = code,
                Address = "Test Address",
                City = "Test City",
                State = "Test State",
                Pincode = "123456",
                ContactEmail = "test@school.com",
                ContactPhone = "+91-9876543210",
                PrincipalName = "Test Principal",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static SchoolCreateDto CreateTestSchoolDto(string name, string code)
        {
            return new SchoolCreateDto
            {
                SchoolName = name,
                SchoolCode = code,
                Address = "Test Address",
                City = "Test City",
                State = "Test State",
                Pincode = "123456",
                ContactEmail = "test@school.com",
                ContactPhone = "+91-9876543210",
                PrincipalName = "Test Principal"
            };
        }

        #endregion
    }
}
