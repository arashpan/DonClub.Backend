using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Donclub.Api.Controllers;
using Donclub.Application.Branches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Donclub.Api.Tests.Controllers
{
	public class BranchesControllerTests
	{
		private readonly Mock<IBranchService> _branchServiceMock;

		public BranchesControllerTests()
		{
			_branchServiceMock = new Mock<IBranchService>();
		}

		private BranchesController CreateController()
		{
			return new BranchesController(_branchServiceMock.Object);
		}

		// =========================================
		//  GetAll
		// =========================================

		[Fact]
		public async Task GetAll_Should_Return_Ok_With_Result()
		{
			// Arrange
			var controller = CreateController();
			var ct = new CancellationTokenSource().Token;

			var expectedList = new List<BranchSummaryDto>
			{
				new BranchSummaryDto(
					Id: 1,
					Name: "Branch 1",
					IsActive: true
				),
				new BranchSummaryDto(
					Id: 2,
					Name: "Branch 2",
					IsActive: false
				)
			};

			_branchServiceMock
				.Setup(s => s.GetAllAsync(ct))
				.ReturnsAsync(expectedList);

			// Act
			ActionResult<IReadOnlyList<BranchSummaryDto>> actionResult =
				await controller.GetAll(ct);

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
			var value = Assert.IsAssignableFrom<IReadOnlyList<BranchSummaryDto>>(okResult.Value);

			Assert.Same(expectedList, value);

			_branchServiceMock.Verify(
				s => s.GetAllAsync(ct),
				Times.Once);
		}

		[Fact]
		public void GetAll_Should_Have_HttpGet_And_AllowAnonymous()
		{
			var method = typeof(BranchesController).GetMethod(nameof(BranchesController.GetAll));
			Assert.NotNull(method);

			var httpGet = method!.GetCustomAttribute<HttpGetAttribute>();
			Assert.NotNull(httpGet);
			Assert.Null(httpGet.Template); // یعنی [HttpGet] بدون route خاص

			var allowAnonymous = method.GetCustomAttribute<AllowAnonymousAttribute>();
			Assert.NotNull(allowAnonymous);
		}

		// =========================================
		//  GetById
		// =========================================

		[Fact]
		public async Task GetById_When_Found_Should_Return_Ok_With_Detail()
		{
			// Arrange
			var controller = CreateController();
			var ct = new CancellationTokenSource().Token;
			var id = 10;

			var rooms = new List<RoomDto>
			{
				new RoomDto(
					Id: 1,
					Name: "Room 1",
					Capacity: 10,
					IsActive: true
				)
			};

			var branchDetail = new BranchDetailDto(
				Id: id,
				Name: "Test Branch",
				Address: "Some address",
				IsActive: true,
				Rooms: rooms
			);

			_branchServiceMock
				.Setup(s => s.GetByIdAsync(id, ct))
				.ReturnsAsync(branchDetail);

			// Act
			ActionResult<BranchDetailDto> actionResult =
				await controller.GetById(id, ct);

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
			var value = Assert.IsType<BranchDetailDto>(okResult.Value);

			Assert.Same(branchDetail, value);

			_branchServiceMock.Verify(
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

			_branchServiceMock
				.Setup(s => s.GetByIdAsync(id, ct))
				.ReturnsAsync((BranchDetailDto?)null);

			// Act
			ActionResult<BranchDetailDto> actionResult =
				await controller.GetById(id, ct);

			// Assert
			Assert.IsType<NotFoundResult>(actionResult.Result);

			_branchServiceMock.Verify(
				s => s.GetByIdAsync(id, ct),
				Times.Once);
		}

		[Fact]
		public void GetById_Should_Have_HttpGet_With_Int_Route()
		{
			var method = typeof(BranchesController).GetMethod(nameof(BranchesController.GetById));
			Assert.NotNull(method);

			var httpGet = method!.GetCustomAttribute<HttpGetAttribute>();
			Assert.NotNull(httpGet);
			Assert.Equal("{id:int}", httpGet.Template);
		}

		// =========================================
		//  Create
		// =========================================

		[Fact]
		public async Task Create_Should_Call_Service_And_Return_CreatedAtAction()
		{
			// Arrange
			var controller = CreateController();
			var ct = new CancellationTokenSource().Token;

			var request = new CreateBranchRequest(
				Name: "New Branch",
				Address: "Tehran"
			);

			var newId = 123;

			_branchServiceMock
				.Setup(s => s.CreateBranchAsync(request, ct))
				.ReturnsAsync(newId);

			// Act
			ActionResult actionResult =
				await controller.Create(request, ct);

			// Assert
			var created = Assert.IsType<CreatedAtActionResult>(actionResult);

			Assert.Equal(nameof(BranchesController.GetById), created.ActionName);
			Assert.NotNull(created.RouteValues);
			Assert.True(created.RouteValues!.ContainsKey("id"));
			Assert.Equal(newId, created.RouteValues["id"]);
			Assert.Null(created.Value); // خود کنترلر null برمی‌گردونه

			_branchServiceMock.Verify(
				s => s.CreateBranchAsync(request, ct),
				Times.Once);
		}

		[Fact]
		public void Create_Should_Have_HttpPost()
		{
			var method = typeof(BranchesController).GetMethod(nameof(BranchesController.Create));
			Assert.NotNull(method);

			var httpPost = method!.GetCustomAttribute<HttpPostAttribute>();
			Assert.NotNull(httpPost);
			Assert.Null(httpPost.Template);
		}

		// =========================================
		//  Update
		// =========================================

		[Fact]
		public async Task Update_Should_Call_Service_And_Return_NoContent()
		{
			// Arrange
			var controller = CreateController();
			var ct = new CancellationTokenSource().Token;
			var id = 5;

			var request = new UpdateBranchRequest(
				Name: "Updated Name",
				Address: "New Address",
				IsActive: true
			);

			_branchServiceMock
				.Setup(s => s.UpdateBranchAsync(id, request, ct))
				.Returns(Task.CompletedTask);

			// Act
			ActionResult actionResult =
				await controller.Update(id, request, ct);

			// Assert
			Assert.IsType<NoContentResult>(actionResult);

			_branchServiceMock.Verify(
				s => s.UpdateBranchAsync(id, request, ct),
				Times.Once);
		}

		[Fact]
		public void Update_Should_Have_HttpPut_With_Int_Route()
		{
			var method = typeof(BranchesController).GetMethod(nameof(BranchesController.Update));
			Assert.NotNull(method);

			var httpPut = method!.GetCustomAttribute<HttpPutAttribute>();
			Assert.NotNull(httpPut);
			Assert.Equal("{id:int}", httpPut.Template);
		}

		// =========================================
		//  Delete
		// =========================================

		[Fact]
		public async Task Delete_Should_Call_Service_And_Return_NoContent()
		{
			// Arrange
			var controller = CreateController();
			var ct = new CancellationTokenSource().Token;
			var id = 7;

			_branchServiceMock
				.Setup(s => s.DeleteBranchAsync(id, ct))
				.Returns(Task.CompletedTask);

			// Act
			ActionResult actionResult =
				await controller.Delete(id, ct);

			// Assert
			Assert.IsType<NoContentResult>(actionResult);

			_branchServiceMock.Verify(
				s => s.DeleteBranchAsync(id, ct),
				Times.Once);
		}

		[Fact]
		public void Delete_Should_Have_HttpDelete_With_Int_Route()
		{
			var method = typeof(BranchesController).GetMethod(nameof(BranchesController.Delete));
			Assert.NotNull(method);

			var httpDelete = method!.GetCustomAttribute<HttpDeleteAttribute>();
			Assert.NotNull(httpDelete);
			Assert.Equal("{id:int}", httpDelete.Template);
		}

		// =========================================
		//  Rooms - AddRoom
		// =========================================

		[Fact]
		public async Task AddRoom_Should_Call_Service_And_Return_CreatedAtAction_With_RoomId()
		{
			// Arrange
			var controller = CreateController();
			var ct = new CancellationTokenSource().Token;

			var branchId = 10;
			var request = new CreateRoomRequest(
				Name: "VIP Room",
				Capacity: 12
			);

			var roomId = 77;

			_branchServiceMock
				.Setup(s => s.AddRoomAsync(branchId, request, ct))
				.ReturnsAsync(roomId);

			// Act
			ActionResult actionResult =
				await controller.AddRoom(branchId, request, ct);

			// Assert
			var created = Assert.IsType<CreatedAtActionResult>(actionResult);

			Assert.Equal(nameof(BranchesController.GetById), created.ActionName);
			Assert.NotNull(created.RouteValues);
			Assert.True(created.RouteValues!.ContainsKey("id"));
			Assert.Equal(branchId, created.RouteValues["id"]);

			Assert.NotNull(created.Value);
			var value = created.Value!;
			var prop = value.GetType().GetProperty("roomId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
			Assert.NotNull(prop);
			var returnedRoomId = prop!.GetValue(value);
			Assert.Equal(roomId, returnedRoomId);

			_branchServiceMock.Verify(
				s => s.AddRoomAsync(branchId, request, ct),
				Times.Once);
		}

		[Fact]
		public void AddRoom_Should_Have_HttpPost_With_Rooms_Route()
		{
			var method = typeof(BranchesController).GetMethod(nameof(BranchesController.AddRoom));
			Assert.NotNull(method);

			var httpPost = method!.GetCustomAttribute<HttpPostAttribute>();
			Assert.NotNull(httpPost);
			Assert.Equal("{branchId:int}/rooms", httpPost.Template);
		}

		// =========================================
		//  Rooms - UpdateRoom
		// =========================================

		[Fact]
		public async Task UpdateRoom_Should_Call_Service_And_Return_NoContent()
		{
			// Arrange
			var controller = CreateController();
			var ct = new CancellationTokenSource().Token;

			var branchId = 20;
			var roomId = 5;

			var request = new UpdateRoomRequest(
				Name: "Updated Room",
				Capacity: 8,
				IsActive: true
			);

			_branchServiceMock
				.Setup(s => s.UpdateRoomAsync(branchId, roomId, request, ct))
				.Returns(Task.CompletedTask);

			// Act
			ActionResult actionResult =
				await controller.UpdateRoom(branchId, roomId, request, ct);

			// Assert
			Assert.IsType<NoContentResult>(actionResult);

			_branchServiceMock.Verify(
				s => s.UpdateRoomAsync(branchId, roomId, request, ct),
				Times.Once);
		}

		[Fact]
		public void UpdateRoom_Should_Have_HttpPut_With_Rooms_Route()
		{
			var method = typeof(BranchesController).GetMethod(nameof(BranchesController.UpdateRoom));
			Assert.NotNull(method);

			var httpPut = method!.GetCustomAttribute<HttpPutAttribute>();
			Assert.NotNull(httpPut);
			Assert.Equal("{branchId:int}/rooms/{roomId:int}", httpPut.Template);
		}

		// =========================================
		//  Rooms - DeleteRoom
		// =========================================

		[Fact]
		public async Task DeleteRoom_Should_Call_Service_And_Return_NoContent()
		{
			// Arrange
			var controller = CreateController();
			var ct = new CancellationTokenSource().Token;

			var branchId = 30;
			var roomId = 9;

			_branchServiceMock
				.Setup(s => s.DeleteRoomAsync(branchId, roomId, ct))
				.Returns(Task.CompletedTask);

			// Act
			ActionResult actionResult =
				await controller.DeleteRoom(branchId, roomId, ct);

			// Assert
			Assert.IsType<NoContentResult>(actionResult);

			_branchServiceMock.Verify(
				s => s.DeleteRoomAsync(branchId, roomId, ct),
				Times.Once);
		}

		[Fact]
		public void DeleteRoom_Should_Have_HttpDelete_With_Rooms_Route()
		{
			var method = typeof(BranchesController).GetMethod(nameof(BranchesController.DeleteRoom));
			Assert.NotNull(method);

			var httpDelete = method!.GetCustomAttribute<HttpDeleteAttribute>();
			Assert.NotNull(httpDelete);
			Assert.Equal("{branchId:int}/rooms/{roomId:int}", httpDelete.Template);
		}

		// =========================================
		//  Controller-level attributes
		// =========================================

		[Fact]
		public void BranchesController_Should_Have_ApiController_And_Route_And_Authorize()
		{
			var type = typeof(BranchesController);

			var apiController = type.GetCustomAttribute<ApiControllerAttribute>();
			Assert.NotNull(apiController);

			var route = type.GetCustomAttribute<RouteAttribute>();
			Assert.NotNull(route);
			Assert.Equal("api/[controller]", route.Template);

			var authorize = type.GetCustomAttribute<AuthorizeAttribute>();
			Assert.NotNull(authorize);
			Assert.Equal("SuperUser,Admin", authorize.Roles);
		}
	}
}
