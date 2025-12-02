using Donclub.Api.Controllers;
using Donclub.Application.Badges;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace Donclub.Api.Tests.Controllers
{
    public class BadgesControllerTests
    {
        private readonly Mock<IBadgeService> _badgeServiceMock;

        public BadgesControllerTests()
        {
            _badgeServiceMock = new Mock<IBadgeService>();
        }

        private BadgesController CreateController(ClaimsPrincipal? user = null)
        {
            var controller = new BadgesController(_badgeServiceMock.Object);

            var httpContext = new DefaultHttpContext();

            if (user != null)
            {
                httpContext.User = user;
            }

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        // =============================
        //  GetMyBadges
        // =============================

        [Fact]
        public async Task GetMyBadges_When_User_Has_Valid_Id_Should_Return_Ok_With_List()
        {
            // Arrange
            var userId = 123L;
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            var controller = CreateController(user);
            var ct = new CancellationTokenSource().Token;

            var expectedList = new List<PlayerBadgeDto>
            {
                new PlayerBadgeDto(
                    Id: 1,
                    BadgeId: 10,
                    BadgeName: "First Login",
                    BadgeCode: "FIRST_LOGIN",
                    Reason: "اولین ورود",
                    IconUrl: "https://example.com/badges/first-login.png",
                    IsRevoked: false,
                    EarnedAtUtc: DateTime.UtcNow.AddDays(-1)
                    )
            };

            _badgeServiceMock
                .Setup(s => s.GetBadgesForUserAsync(userId, ct))
                .ReturnsAsync(expectedList);

            // Act
            ActionResult<IReadOnlyList<PlayerBadgeDto>> actionResult =
                await controller.GetMyBadges(ct);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var value = Assert.IsAssignableFrom<IReadOnlyList<PlayerBadgeDto>>(okResult.Value);

            Assert.Same(expectedList, value);

            _badgeServiceMock.Verify(
                s => s.GetBadgesForUserAsync(userId, ct),
                Times.Once);
        }

        [Fact]
        public async Task GetMyBadges_When_User_Has_No_NameIdentifier_Claim_Should_Return_Unauthorized()
        {
            // Arrange
            // کاربر بدون Claim
            var identity = new ClaimsIdentity(authenticationType: "TestAuth");
            var user = new ClaimsPrincipal(identity);

            var controller = CreateController(user);
            var ct = new CancellationTokenSource().Token;

            // Act
            ActionResult<IReadOnlyList<PlayerBadgeDto>> actionResult =
                await controller.GetMyBadges(ct);

            // Assert
            Assert.IsType<UnauthorizedResult>(actionResult.Result);

            _badgeServiceMock.Verify(
                s => s.GetBadgesForUserAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetMyBadges_When_UserId_Claim_Is_Not_Long_Should_Return_Unauthorized()
        {
            // Arrange
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "NotALong") // مقدار غیرقابل تبدیل به long
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            var controller = CreateController(user);
            var ct = new CancellationTokenSource().Token;

            // Act
            ActionResult<IReadOnlyList<PlayerBadgeDto>> actionResult =
                await controller.GetMyBadges(ct);

            // Assert
            Assert.IsType<UnauthorizedResult>(actionResult.Result);

            _badgeServiceMock.Verify(
                s => s.GetBadgesForUserAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetMyBadges_Should_Use_CancellationToken_From_Parameter()
        {
            // Arrange
            var userId = 456L;
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            var controller = CreateController(user);

            using var cts = new CancellationTokenSource();
            var ct = cts.Token;

            _badgeServiceMock
                .Setup(s => s.GetBadgesForUserAsync(userId, ct))
                .ReturnsAsync(Array.Empty<PlayerBadgeDto>());

            // Act
            ActionResult<IReadOnlyList<PlayerBadgeDto>> actionResult =
                await controller.GetMyBadges(ct);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.IsAssignableFrom<IReadOnlyList<PlayerBadgeDto>>(okResult.Value);

            _badgeServiceMock.Verify(
                s => s.GetBadgesForUserAsync(userId, ct),
                Times.Once);
        }
    }
}
