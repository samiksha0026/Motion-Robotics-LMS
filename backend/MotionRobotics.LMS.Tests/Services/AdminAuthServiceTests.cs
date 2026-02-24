using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;
using Microsoft.Extensions.Configuration;

namespace MotionRobotics.LMS.Tests.Services
{
    public class AdminAuthServiceTests
    {
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly AdminAuthService _sut;

        public AdminAuthServiceTests()
        {
            // Setup UserManager mock
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            // Setup SignInManager mock
            var contextAccessorMock = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
                _userManagerMock.Object,
                contextAccessorMock.Object,
                userPrincipalFactoryMock.Object, null!, null!, null!, null!);

            // Setup Configuration mock
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["Jwt:SecretKey"]).Returns("ThisIsAVerySecureSecretKeyForTesting123456!");
            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

            _sut = new AdminAuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _configurationMock.Object);
        }

        [Fact]
        public async Task LoginAsync_WithValidAdminCredentials_ReturnsToken()
        {
            // Arrange
            var request = new AdminLoginRequestDto
            {
                Email = "admin@motionrobotics.com",
                Password = "Admin@123"
            };

            var user = new IdentityUser
            {
                Id = "test-user-id",
                UserName = "admin@motionrobotics.com",
                Email = "admin@motionrobotics.com"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false))
                .ReturnsAsync(SignInResult.Success);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin" });

            // Act
            var result = await _sut.LoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result!.Token.Should().NotBeNullOrEmpty();
            result.AdminEmail.Should().Be(user.Email);
            result.Roles.Should().Contain("Admin");
            result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidEmail_ReturnsNull()
        {
            // Arrange
            var request = new AdminLoginRequestDto
            {
                Email = "nonexistent@test.com",
                Password = "Password123"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((IdentityUser?)null);

            // Act
            var result = await _sut.LoginAsync(request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ReturnsNull()
        {
            // Arrange
            var request = new AdminLoginRequestDto
            {
                Email = "admin@motionrobotics.com",
                Password = "WrongPassword"
            };

            var user = new IdentityUser
            {
                Id = "test-user-id",
                UserName = "admin@motionrobotics.com",
                Email = "admin@motionrobotics.com"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false))
                .ReturnsAsync(SignInResult.Failed);

            // Act
            var result = await _sut.LoginAsync(request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_WithNonAdminUser_ReturnsNull()
        {
            // Arrange
            var request = new AdminLoginRequestDto
            {
                Email = "teacher@test.com",
                Password = "Teacher@123"
            };

            var user = new IdentityUser
            {
                Id = "teacher-user-id",
                UserName = "teacher@test.com",
                Email = "teacher@test.com"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false))
                .ReturnsAsync(SignInResult.Success);

            // User has Teacher role, not Admin
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Teacher" });

            // Act
            var result = await _sut.LoginAsync(request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_GeneratesValidJwtToken()
        {
            // Arrange
            var request = new AdminLoginRequestDto
            {
                Email = "admin@motionrobotics.com",
                Password = "Admin@123"
            };

            var user = new IdentityUser
            {
                Id = "test-user-id",
                UserName = "admin@motionrobotics.com",
                Email = "admin@motionrobotics.com"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false))
                .ReturnsAsync(SignInResult.Success);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin" });

            // Act
            var result = await _sut.LoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result!.Token.Should().NotBeNullOrEmpty();
            // JWT tokens have 3 parts separated by dots
            result.Token.Split('.').Should().HaveCount(3);
        }
    }
}
