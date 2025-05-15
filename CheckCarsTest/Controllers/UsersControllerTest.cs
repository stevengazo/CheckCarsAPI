using CheckCarsAPI.Controllers;
using CheckCarsAPI.Models;
using CheckCarsAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CheckCarsAPI.Controllers;
public class UsersControllerTests
{
    private Mock<UserManager<UserApp>> GetMockUserManager()
    {
        var store = new Mock<IUserStore<UserApp>>();
        return new Mock<UserManager<UserApp>>(store.Object, null, null, null, null, null, null, null, null);
    }

    private UsersController GetControllerWithUsers(List<UserApp> userList)
    {
        var userManager = GetMockUserManager();
        userManager.Setup(um => um.Users).Returns(userList.AsQueryable());

        var signInManager = new Mock<SignInManager<UserApp>>(
            userManager.Object,
            Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<UserApp>>(),
            null, null, null, null);

        var config = new Mock<IConfiguration>();
        var emailService = new Mock<EmailService>(Mock.Of<IConfiguration>());

        return new UsersController(userManager.Object, signInManager.Object, config.Object, emailService.Object);
    }

    [Fact]
    public void GetUsers_ReturnsUsers()
    {
        var users = new List<UserApp>
        {
            new UserApp { Id = "1", Email = "user1@example.com" },
            new UserApp { Id = "2", Email = "user2@example.com" }
        };

        var controller = GetControllerWithUsers(users);

        var result = controller.GetUsers();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUsers = Assert.IsAssignableFrom<IEnumerable<UserApp>>(okResult.Value);
        Assert.Equal(2, returnedUsers.Count());
    }

    [Fact]
    public async Task GetUser_ReturnsUser_WhenExists()
    {
        var user = new UserApp { Id = "1", Email = "test@example.com" };
        var userManager = GetMockUserManager();
        userManager.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);

        var controller = GetControllerWithUsers(new List<UserApp>());
        controller = new UsersController(userManager.Object, null, null, null);

        var result = await controller.GetUser("1");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUser = Assert.IsType<UserApp>(okResult.Value);
        Assert.Equal("test@example.com", returnedUser.Email);
    }

    [Fact]
    public async Task GetUser_ReturnsNotFound_WhenMissing()
    {
        var userManager = GetMockUserManager();
        userManager.Setup(m => m.FindByIdAsync("1")).ReturnsAsync((UserApp)null);

        var controller = new UsersController(userManager.Object, null, null, null);

        var result = await controller.GetUser("1");

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task AddUser_ReturnsOk_WhenValid()
    {
        var user = new UserApp { Email = "new@example.com" };
        var userManager = GetMockUserManager();
        userManager.Setup(m => m.CreateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var controller = new UsersController(userManager.Object, null, null, null);

        var result = await controller.AddUser(user);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUser = Assert.IsType<UserApp>(okResult.Value);
        Assert.Equal("new@example.com", returnedUser.Email);
    }

    [Fact]
    public async Task AddUser_ReturnsBadRequest_WhenNull()
    {
        var controller = new UsersController(null, null, null, null);
        var result = await controller.AddUser(null);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid user data", badRequest.Value);
    }

    [Fact]
    public async Task UpdateUser_ReturnsOk_WhenUpdated()
    {
        var existingUser = new UserApp { Id = "1", Email = "old@example.com" };
        var updatedUser = new UserApp { Id = "1", Email = "new@example.com", UserName = "newuser" };

        var userManager = GetMockUserManager();
        userManager.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(existingUser);
        userManager.Setup(m => m.UpdateAsync(It.IsAny<UserApp>())).ReturnsAsync(IdentityResult.Success);

        var controller = new UsersController(userManager.Object, null, null, null);
        var result = await controller.UpdateUser("1", updatedUser);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var user = Assert.IsType<UserApp>(okResult.Value);
        Assert.Equal("new@example.com", user.Email);
    }

    [Fact]
    public async Task DeleteUser_ReturnsOk_WhenDeleted()
    {
        var user = new UserApp { Id = "1" };
        var userManager = GetMockUserManager();
        userManager.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);
        userManager.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        var controller = new UsersController(userManager.Object, null, null, null);

        var result = await controller.DeleteUser("1");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("User deleted successfully", okResult.Value);
    }

    [Fact]
    public async Task DeleteUser_ReturnsNotFound_WhenMissing()
    {
        var userManager = GetMockUserManager();
        userManager.Setup(m => m.FindByIdAsync("1")).ReturnsAsync((UserApp)null);

        var controller = new UsersController(userManager.Object, null, null, null);
        var result = await controller.DeleteUser("1");

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
