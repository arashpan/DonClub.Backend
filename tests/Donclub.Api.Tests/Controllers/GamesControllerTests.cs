using Donclub.Api.Controllers;
using Donclub.Application.Games;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;

namespace Donclub.Api.Tests.Controllers
{
    public class GamesControllerTests
    {
        private readonly Mock<IGameService> _gameServiceMock;

        public GamesControllerTests()
        {
            _gameServiceMock = new Mock<IGameService>();
        }

        private GamesController CreateController()
        {
            return new GamesController(_gameServiceMock.Object);
        }

        // =========================================
        //  Controller-level attributes
        // =========================================

        [Fact]
        public void GamesController_Should_Have_ApiController_Route_And_Authorize()
        {
            var type = typeof(GamesController);

            var apiController = type.GetCustomAttribute<ApiControllerAttribute>();
            Assert.NotNull(apiController);

            var route = type.GetCustomAttribute<RouteAttribute>();
            Assert.NotNull(route);
            Assert.Equal("api/[controller]", route.Template);

            var authorize = type.GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(authorize);
            Assert.Equal("Admin,SuperUser", authorize.Roles);
        }

        // =========================================
        //  Games - GetAll
        // =========================================

        [Fact]
        public async Task GetAll_Should_Return_Ok_With_List()
        {
            // Arrange
            var controller = CreateController();
            var ct = new CancellationTokenSource().Token;

            var expected = new List<GameSummaryDto>
            {
                new GameSummaryDto(1, "Game 1", true),
                new GameSummaryDto(2, "Game 2", false)
            };

            _gameServiceMock
                .Setup(s => s.GetAllAsync(ct))
                .ReturnsAsync(expected);

            // Act
            ActionResult<IReadOnlyList<GameSummaryDto>> actionResult =
                await controller.GetAll(ct);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var value = Assert.IsAssignableFrom<IReadOnlyList<GameSummaryDto>>(okResult.Value);

            Assert.Same(expected, value);

            _gameServiceMock.Verify(
                s => s.GetAllAsync(ct),
                Times.Once);
        }

        [Fact]
        public void GetAll_Should_Have_HttpGet_And_AllowAnonymous()
        {
            var method = typeof(GamesController).GetMethod(nameof(GamesController.GetAll));
            Assert.NotNull(method);

            var httpGet = method!.GetCustomAttribute<HttpGetAttribute>();
            Assert.NotNull(httpGet);
            Assert.Null(httpGet.Template); // یعنی [HttpGet] ساده

            var allowAnonymous = method.GetCustomAttribute<AllowAnonymousAttribute>();
            Assert.NotNull(allowAnonymous);
        }

        // =========================================
        //  Games - GetById
        // =========================================

        [Fact]
        public async Task GetById_When_Found_Should_Return_Ok_With_Detail()
        {
            // Arrange
            var controller = CreateController();
            var ct = new CancellationTokenSource().Token;
            var id = 10;

            var roles = new List<GameRoleDto>
            {
                new GameRoleDto(
                    Id: 1,
                    Name: "Role 1",
                    Team: 1,
                    Description: "desc"
                )
            };

            var scenarios = new List<ScenarioDto>
            {
                new ScenarioDto(
                    Id: 1,
                    Name: "Scenario 1",
                    PlayerCount: 5,
                    Roles: new List<ScenarioRoleDto>()
                )
            };

            var detail = new GameDetailDto(
                Id: id,
                Name: "Test Game",
                Description: "Test",
                Roles: roles,
                Scenarios: scenarios
            );

            _gameServiceMock
                .Setup(s => s.GetByIdAsync(id, ct))
                .ReturnsAsync(detail);

            // Act
            ActionResult<GameDetailDto> actionResult =
                await controller.GetById(id, ct);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var value = Assert.IsType<GameDetailDto>(okResult.Value);

            Assert.Same(detail, value);

            _gameServiceMock.Verify(
                s => s.GetByIdAsync(id, ct),
                Times.Once);
        }

        [Fact]
        public async Task GetById_When_NotFound_Should_Return_NotFound()
        {
            // Arrange
            var controller = CreateController();
            var ct = new CancellationTokenSource().Token;
            var id = 999;

            _gameServiceMock
                .Setup(s => s.GetByIdAsync(id, ct))
                .ReturnsAsync((GameDetailDto?)null);

            // Act
            ActionResult<GameDetailDto> actionResult =
                await controller.GetById(id, ct);

            // Assert
            Assert.IsType<NotFoundResult>(actionResult.Result);

            _gameServiceMock.Verify(
                s => s.GetByIdAsync(id, ct),
                Times.Once);
        }

        [Fact]
        public void GetById_Should_Have_HttpGet_With_Int_Route()
        {
            var method = typeof(GamesController).GetMethod(nameof(GamesController.GetById));
            Assert.NotNull(method);

            var httpGet = method!.GetCustomAttribute<HttpGetAttribute>();
            Assert.NotNull(httpGet);
            Assert.Equal("{id:int}", httpGet.Template);
        }

        // =========================================
        //  Games - Create
        // =========================================

        [Fact]
        public async Task Create_Should_Call_Service_And_Return_CreatedAtAction()
        {
            // Arrange
            var controller = CreateController();
            var ct = new CancellationTokenSource().Token;

            var request = new CreateGameRequest(
                Name: "New Game",
                Description: "Some description"
            );

            var newId = 123;

            _gameServiceMock
                .Setup(s => s.CreateGameAsync(request, ct))
                .ReturnsAsync(newId);

            // Act
            ActionResult actionResult =
                await controller.Create(request, ct);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(actionResult);

            Assert.Equal(nameof(GamesController.GetById), created.ActionName);
            Assert.NotNull(created.RouteValues);
            Assert.True(created.RouteValues!.ContainsKey("id"));
            Assert.Equal(newId, created.RouteValues["id"]);
            Assert.Null(created.Value); // کنترلر null برمی‌گردونه

            _gameServiceMock.Verify(
                s => s.CreateGameAsync(request, ct),
                Times.Once);
        }

        [Fact]
        public void Create_Should_Have_HttpPost()
        {
            var method = typeof(GamesController).GetMethod(nameof(GamesController.Create));
            Assert.NotNull(method);

            var httpPost = method!.GetCustomAttribute<HttpPostAttribute>();
            Assert.NotNull(httpPost);
            Assert.Null(httpPost.Template);
        }

        // =========================================
        //  Games - Update
        // =========================================

        [Fact]
        public async Task Update_Should_Call_Service_And_Return_NoContent()
        {
            // Arrange
            var controller = CreateController();
            var ct = new CancellationTokenSource().Token;
            var id = 10;

            var request = new UpdateGameRequest(
                Name: "Updated Name",
                Description: "Updated Desc",
                IsActive: true
            );

            _gameServiceMock
                .Setup(s => s.UpdateGameAsync(id, request, ct))
                .Returns(Task.CompletedTask);

            // Act
            ActionResult actionResult =
                await controller.Update(id, request, ct);

            // Assert
            Assert.IsType<NoContentResult>(actionResult);

            _gameServiceMock.Verify(
                s => s.UpdateGameAsync(id, request, ct),
                Times.Once);
        }

        [Fact]
        public void Update_Should_Have_HttpPut_With_Int_Route()
        {
            var method = typeof(GamesController).GetMethod(nameof(GamesController.Update));
            Assert.NotNull(method);

            var httpPut = method!.GetCustomAttribute<HttpPutAttribute>();
            Assert.NotNull(httpPut);
            Assert.Equal("{id:int}", httpPut.Template);
        }

        // =========================================
        //  Games - Delete
        // =========================================

        [Fact]
        public async Task Delete_Should_Call_Service_And_Return_NoContent()
        {
            // Arrange
            var controller = CreateController();
            var ct = new CancellationTokenSource().Token;
            var id = 15;

            _gameServiceMock
                .Setup(s => s.DeleteGameAsync(id, ct))
                .Returns(Task.CompletedTask);

            // Act
            ActionResult actionResult =
                await controller.Delete(id, ct);

            // Assert
            Assert.IsType<NoContentResult>(actionResult);

            _gameServiceMock.Verify(
                s => s.DeleteGameAsync(id, ct),
                Times.Once);
        }

        [Fact]
        public void Delete_Should_Have_HttpDelete_With_Int_Route()
        {
            var method = typeof(GamesController).GetMethod(nameof(GamesController.Delete));
            Assert.NotNull(method);

            var httpDelete = method!.GetCustomAttribute<HttpDeleteAttribute>();
            Assert.NotNull(httpDelete);
            Assert.Equal("{id:int}", httpDelete.Template);
        }

        // =========================================
        //  Game Roles - AddRole
        // =========================================

        [Fact]
        public async Task AddRole_Should_Return_Ok_With_RoleId()
        {
            // Arrange
            var controller = CreateController();
            var ct = new CancellationTokenSource().Token;

            var gameId = 5;
            var request = new CreateGameRoleRequest(
                Name: "New Role",
                Team: 1,
                Description: "role desc"
            );

            var roleId = 77;

            _gameServiceMock
                .Setup(s => s.AddRoleAsync(gameId, request, ct))
                .ReturnsAsync(roleId);

            // Act
            ActionResult actionResult =
                await controller.AddRole(gameId, request, ct);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.NotNull(okResult.Value);

            var value = okResult.Value!;
            var prop = value.GetType().GetProperty("roleId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            Assert.NotNull(prop);
            var returnedRoleId = prop!.GetValue(value);
            Assert.Equal(roleId, returnedRoleId);

            _gameServiceMock.Verify(
                s => s.AddRoleAsync(gameId, request, ct),
                Times.Once);
        }

        [Fact]
        public void AddRole_Should_Have_HttpPost_With_Roles_Route()
        {
            var method = typeof(GamesController).GetMethod(nameof(GamesController.AddRole));
            Assert.NotNull(method);

            var httpPost = method!.GetCustomAttribute<HttpPostAttribute>();
            Assert.NotNull(httpPost);
            Assert.Equal("{gameId:int}/roles", httpPost.Template);
        }

        // =========================================
        //  Game Roles - UpdateRole
        // =========================================

        [Fact]
        public async Task UpdateRole_Should_Call_Service_And_Return_NoContent()
        {
            // Arrange
            var controller = CreateController();
            var ct = new CancellationTokenSource().Token;

            var gameId = 10;
            var roleId = 3;

            var request = new UpdateGameRoleRequest(
                Name: "Updated Role",
                Team: 2,
                Description: "updated desc",
                IsActive: true
            );

            _gameServiceMock
                .Setup(s => s.UpdateRoleAsync(gameId, roleId, request, ct))
                .Returns(Task.CompletedTask);

            // Act
            ActionResult actionResult =
                await controller.UpdateRole(gameId, roleId, request, ct);

            // Assert
            Assert.IsType<NoContentResult>(actionResult);

            _gameServiceMock.Verify(
                s => s.UpdateRoleAsync(gameId, roleId, request, ct),
                Times.Once);
        }

        [Fact]
        public void UpdateRole_Should_Have_HttpPut_With_Roles_Route()
        {
            var method = typeof(GamesController).GetMethod(nameof(GamesController.UpdateRole));
            Assert.NotNull(method);

            var httpPut = method!.GetCustomAttribute<HttpPutAttribute>();
            Assert.NotNull(httpPut);
            Assert.Equal("{gameId:int}/roles/{roleId:int}", httpPut.Template);
        }

        // =========================================
        //  Game Roles - DeleteRole
        // =========================================

        [Fact]
        public async Task DeleteRole_Should_Call_Service_And_Return_NoContent()
        {
            // Arrange
            var controller = CreateController();
            var ct = new CancellationTokenSource().Token;

            var gameId = 20;
            var roleId = 4;

            _gameServiceMock
                .Setup(s => s.DeleteRoleAsync(gameId, roleId, ct))
                .Returns(Task.CompletedTask);

            // Act
            ActionResult actionResult =
                await controller.DeleteRole(gameId, roleId, ct);

            // Assert
            Assert.IsType<NoContentResult>(actionResult);

            _gameServiceMock.Verify(
                s => s.DeleteRoleAsync(gameId, roleId, ct),
                Times.Once);
        }

        [Fact]
        public void DeleteRole_Should_Have_HttpDelete_With_Roles_Route()
        {
            var method = typeof(GamesController).GetMethod(nameof(GamesController.DeleteRole));
            Assert.NotNull(method);

            var httpDelete = method!.GetCustomAttribute<HttpDeleteAttribute>();
            Assert.NotNull(httpDelete);
            Assert.Equal("{gameId:int}/roles/{roleId:int}", httpDelete.Template);
        }

        // =========================================
        //  Scenarios - AddScenario
        // =========================================

        [Fact]
        public async Task AddScenario_Should_Return_Ok_With_ScenarioId()
        {
            // Arrange
            var controller = CreateController();
            var ct = new CancellationTokenSource().Token;

            var gameId = 7;
            var request = new CreateScenarioRequest(
                Name: "New Scenario",
                PlayerCount: 5,
                Description: "scenario desc"
            );

            var scenarioId = 90;

            _gameServiceMock
                .Setup(s => s.AddScenarioAsync(gameId, request, ct))
                .ReturnsAsync(scenarioId);

            // Act
            ActionResult actionResult =
                await controller.AddScenario(gameId, request, ct);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.NotNull(okResult.Value);

            var value = okResult.Value!;
            var prop = value.GetType().GetProperty("scenarioId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            Assert.NotNull(prop);
            var returnedScenarioId = prop!.GetValue(value);
            Assert.Equal(scenarioId, returnedScenarioId);

            _gameServiceMock.Verify(
                s => s.AddScenarioAsync(gameId, request, ct),
                Times.Once);
        }

        [Fact]
        public void AddScenario_Should_Have_HttpPost_With_Scenarios_Route()
        {
            var method = typeof(GamesController).GetMethod(nameof(GamesController.AddScenario));
            Assert.NotNull(method);

            var httpPost = method!.GetCustomAttribute<HttpPostAttribute>();
            Assert.NotNull(httpPost);
            Assert.Equal("{gameId:int}/scenarios", httpPost.Template);
        }

        // =========================================
        //  Scenarios - UpdateScenario
        // =========================================

        [Fact]
        public async Task UpdateScenario_Should_Call_Service_And_Return_NoContent()
        {
            // Arrange
            var controller = CreateController();
            var ct = new CancellationTokenSource().Token;

            var gameId = 8;
            var scenarioId = 2;

            var request = new UpdateScenarioRequest(
                Name: "Updated Scenario",
                PlayerCount: 6,
                Description: "updated desc",
                IsActive: true
            );

            _gameServiceMock
                .Setup(s => s.UpdateScenarioAsync(gameId, scenarioId, request, ct))
                .Returns(Task.CompletedTask);

            // Act
            ActionResult actionResult =
                await controller.UpdateScenario(gameId, scenarioId, request, ct);

            // Assert
            Assert.IsType<NoContentResult>(actionResult);

            _gameServiceMock.Verify(
                s => s.UpdateScenarioAsync(gameId, scenarioId, request, ct),
                Times.Once);
        }

        [Fact]
        public void UpdateScenario_Should_Have_HttpPut_With_Scenarios_Route()
        {
            var method = typeof(GamesController).GetMethod(nameof(GamesController.UpdateScenario));
            Assert.NotNull(method);

            var httpPut = method!.GetCustomAttribute<HttpPutAttribute>();
            Assert.NotNull(httpPut);
            Assert.Equal("{gameId:int}/scenarios/{scenarioId:int}", httpPut.Template);
        }

        // =========================================
        //  Scenarios - DeleteScenario
        // =========================================

        [Fact]
        public async Task DeleteScenario_Should_Call_Service_And_Return_NoContent()
        {
            // Arrange
            var controller = CreateController();
            var ct = new CancellationTokenSource().Token;

            var gameId = 9;
            var scenarioId = 3;

            _gameServiceMock
                .Setup(s => s.DeleteScenarioAsync(gameId, scenarioId, ct))
                .Returns(Task.CompletedTask);

            // Act
            ActionResult actionResult =
                await controller.DeleteScenario(gameId, scenarioId, ct);

            // Assert
            Assert.IsType<NoContentResult>(actionResult);

            _gameServiceMock.Verify(
                s => s.DeleteScenarioAsync(gameId, scenarioId, ct),
                Times.Once);
        }

        [Fact]
        public void DeleteScenario_Should_Have_HttpDelete_With_Scenarios_Route()
        {
            var method = typeof(GamesController).GetMethod(nameof(GamesController.DeleteScenario));
            Assert.NotNull(method);

            var httpDelete = method!.GetCustomAttribute<HttpDeleteAttribute>();
            Assert.NotNull(httpDelete);
            Assert.Equal("{gameId:int}/scenarios/{scenarioId:int}", httpDelete.Template);
        }

        // =========================================
        //  Scenario Roles - SetScenarioRoles
        // =========================================

        [Fact]
        public async Task SetScenarioRoles_Should_Call_Service_And_Return_NoContent()
        {
            // Arrange
            var controller = CreateController();
            var ct = new CancellationTokenSource().Token;

            var gameId = 11;
            var scenarioId = 4;

            var roles = new List<ScenarioRoleInput>
            {
                new ScenarioRoleInput(GameRoleId: 1, Count: 2),
                new ScenarioRoleInput(GameRoleId: 2, Count: 3)
            };

            var request = new SetScenarioRolesRequest(roles);

            _gameServiceMock
                .Setup(s => s.SetScenarioRolesAsync(gameId, scenarioId, request, ct))
                .Returns(Task.CompletedTask);

            // Act
            ActionResult actionResult =
                await controller.SetScenarioRoles(gameId, scenarioId, request, ct);

            // Assert
            Assert.IsType<NoContentResult>(actionResult);

            _gameServiceMock.Verify(
                s => s.SetScenarioRolesAsync(gameId, scenarioId, request, ct),
                Times.Once);
        }

        [Fact]
        public void SetScenarioRoles_Should_Have_HttpPost_With_Roles_Route()
        {
            var method = typeof(GamesController).GetMethod(nameof(GamesController.SetScenarioRoles));
            Assert.NotNull(method);

            var httpPost = method!.GetCustomAttribute<HttpPostAttribute>();
            Assert.NotNull(httpPost);
            Assert.Equal("{gameId:int}/scenarios/{scenarioId:int}/roles", httpPost.Template);
        }
    }
}
