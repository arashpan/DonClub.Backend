using Donclub.Api.Controllers;
using Donclub.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;

namespace Donclub.Api.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
        }

        private AuthController CreateController()
        {
            return new AuthController(_authServiceMock.Object);
        }

        // =============================
        //  RequestOtp
        // =============================

        [Fact]
        public async Task RequestOtp_Should_Return_Ok_With_Result()
        {
            // Arrange
            var controller = CreateController();
            var phoneNumber = "09123456789";
            var ct = new CancellationTokenSource().Token;

            var expected = new RequestOtpResultDto(
                PhoneNumber: phoneNumber,
                ExpiresAtUtc: DateTime.UtcNow.AddMinutes(2)
            );

            _authServiceMock
                .Setup(s => s.RequestOtpAsync(phoneNumber, ct))
                .ReturnsAsync(expected);

            // توجه: این نوع باید دقیقا مثل رکورد داخل AuthController باشه:
            // public record RequestOtpRequest(string PhoneNumber);
            var request = new AuthController.RequestOtpRequest(phoneNumber);

            // Act
            ActionResult<RequestOtpResultDto> actionResult =
                await controller.RequestOtp(request, ct);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var value = Assert.IsType<RequestOtpResultDto>(okResult.Value);

            Assert.Equal(expected, value);

            _authServiceMock.Verify(
                s => s.RequestOtpAsync(phoneNumber, ct),
                Times.Once);
        }

        // =============================
        //  VerifyOtp
        // =============================

        [Fact]
        public async Task VerifyOtp_Should_Return_Ok_With_AuthResult()
        {
            // Arrange
            var controller = CreateController();
            var phoneNumber = "09123456789";
            var code = "123456";
            var ct = new CancellationTokenSource().Token;

            var user = new UserDto(
                Id: 1,
                PhoneNumber: phoneNumber,
                DisplayName: "Test User",
                UserCode: "111111",
                Roles: new[] { "User" }
            );

            var tokens = new AuthTokensDto(
                AccessToken: "access-token",
                RefreshToken: "refresh-token",
                AccessTokenExpiresAtUtc: DateTime.UtcNow.AddMinutes(15),
                RefreshTokenExpiresAtUtc: DateTime.UtcNow.AddDays(7)
            );

            var expected = new AuthResultDto(user, tokens);

            _authServiceMock
                .Setup(s => s.VerifyOtpAsync(phoneNumber, code, ct))
                .ReturnsAsync(expected);

            // این هم باید با رکورد داخل AuthController یکی باشه:
            // public record VerifyOtpRequest(string PhoneNumber, string Code);
            var request = new AuthController.VerifyOtpRequest(phoneNumber, code);

            // Act
            ActionResult<AuthResultDto> actionResult =
                await controller.VerifyOtp(request, ct);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var value = Assert.IsType<AuthResultDto>(okResult.Value);

            Assert.Equal(expected, value);

            _authServiceMock.Verify(
                s => s.VerifyOtpAsync(phoneNumber, code, ct),
                Times.Once);
        }

        [Fact]
        public async Task VerifyOtp_When_InvalidOperationException_Should_Return_BadRequest_With_Message()
        {
            // Arrange
            var controller = CreateController();
            var phoneNumber = "09123456789";
            var code = "000000";
            var ct = new CancellationTokenSource().Token;

            var errorMessage = "Invalid OTP";

            _authServiceMock
                .Setup(s => s.VerifyOtpAsync(phoneNumber, code, ct))
                .ThrowsAsync(new InvalidOperationException(errorMessage));

            var request = new AuthController.VerifyOtpRequest(phoneNumber, code);

            // Act
            ActionResult<AuthResultDto> actionResult =
                await controller.VerifyOtp(request, ct);

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            var payload = badResult.Value;

            Assert.NotNull(payload);

            // چون anonymous object هست (new { message = ex.Message })
            // با reflection پراپرتی message رو می‌گیریم
            var messageProp = payload!.GetType().GetProperty("message", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(messageProp);

            var actualMessage = messageProp!.GetValue(payload) as string;
            Assert.Equal(errorMessage, actualMessage);

            _authServiceMock.Verify(
                s => s.VerifyOtpAsync(phoneNumber, code, ct),
                Times.Once);
        }

        // =============================
        //  Refresh
        // =============================

        [Fact]
        public async Task Refresh_Should_Return_Ok_With_AuthResult()
        {
            // Arrange
            var controller = CreateController();
            var refreshToken = "dummy-refresh-token";
            var ct = new CancellationTokenSource().Token;

            var user = new UserDto(
                Id: 2,
                PhoneNumber: "09999999999",
                DisplayName: "Refresh User",
                UserCode: "999999",
                Roles: new[] { "User" }
            );

            var tokens = new AuthTokensDto(
                AccessToken: "new-access-token",
                RefreshToken: "new-refresh-token",
                AccessTokenExpiresAtUtc: DateTime.UtcNow.AddMinutes(15),
                RefreshTokenExpiresAtUtc: DateTime.UtcNow.AddDays(7)
            );

            var expected = new AuthResultDto(user, tokens);

            _authServiceMock
                .Setup(s => s.RefreshAsync(refreshToken, ct))
                .ReturnsAsync(expected);

            // رکورد داخل کنترلر:
            // public record RefreshRequest(string RefreshToken);
            var request = new AuthController.RefreshRequest(refreshToken);

            // Act
            ActionResult<AuthResultDto> actionResult =
                await controller.Refresh(request, ct);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var value = Assert.IsType<AuthResultDto>(okResult.Value);

            Assert.Equal(expected, value);

            _authServiceMock.Verify(
                s => s.RefreshAsync(refreshToken, ct),
                Times.Once);
        }

        [Fact]
        public async Task Refresh_When_InvalidOperationException_Should_Return_BadRequest_With_Message()
        {
            // Arrange
            var controller = CreateController();
            var refreshToken = "invalid-refresh-token";
            var ct = new CancellationTokenSource().Token;

            var errorMessage = "Invalid refresh token";

            _authServiceMock
                .Setup(s => s.RefreshAsync(refreshToken, ct))
                .ThrowsAsync(new InvalidOperationException(errorMessage));

            var request = new AuthController.RefreshRequest(refreshToken);

            // Act
            ActionResult<AuthResultDto> actionResult =
                await controller.Refresh(request, ct);

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            var payload = badResult.Value;

            Assert.NotNull(payload);

            var messageProp = payload!.GetType().GetProperty("message", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(messageProp);

            var actualMessage = messageProp!.GetValue(payload) as string;
            Assert.Equal(errorMessage, actualMessage);

            _authServiceMock.Verify(
                s => s.RefreshAsync(refreshToken, ct),
                Times.Once);
        }

        // =============================
        //  Attribute-based tests
        // =============================

        [Fact]
        public void AuthController_Should_Have_AllowAnonymous_Attribute()
        {
            var attr = typeof(AuthController).GetCustomAttribute<AllowAnonymousAttribute>();
            Assert.NotNull(attr);
        }

        [Fact]
        public void RequestOtp_Should_Have_HttpPost_With_Correct_Route()
        {
            var method = typeof(AuthController).GetMethod(nameof(AuthController.RequestOtp));
            Assert.NotNull(method);

            var httpPost = method!.GetCustomAttribute<HttpPostAttribute>();
            Assert.NotNull(httpPost);
            Assert.Equal("request-otp", httpPost!.Template);
        }

        [Fact]
        public void VerifyOtp_Should_Have_HttpPost_With_Correct_Route()
        {
            var method = typeof(AuthController).GetMethod(nameof(AuthController.VerifyOtp));
            Assert.NotNull(method);

            var httpPost = method!.GetCustomAttribute<HttpPostAttribute>();
            Assert.NotNull(httpPost);
            Assert.Equal("verify-otp", httpPost!.Template);
        }

        [Fact]
        public void Refresh_Should_Have_HttpPost_With_Correct_Route()
        {
            var method = typeof(AuthController).GetMethod(nameof(AuthController.Refresh));
            Assert.NotNull(method);

            var httpPost = method!.GetCustomAttribute<HttpPostAttribute>();
            Assert.NotNull(httpPost);
            Assert.Equal("refresh", httpPost!.Template);
        }
    }
}
