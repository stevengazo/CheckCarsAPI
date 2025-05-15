using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using CheckCarsAPI.Controllers;
using CheckCarsAPI.Services;
using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

using Microsoft.Extensions.Logging;

namespace CheckCarsAPI.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<UserApp>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _rolManagerMock;
        private readonly Mock<SignInManager<UserApp>> _signInManagerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<EmailService> _emailServiceMock;
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<ILogger<AccountController>> _loggerMock;
       

        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            var userStore = new Mock<IUserStore<UserApp>>();
            _userManagerMock = new Mock<UserManager<UserApp>>(userStore.Object, null, null, null, null, null, null, null, null);
            _rolManagerMock = new Mock<RoleManager<IdentityRole>>(
                new Mock<IRoleStore<IdentityRole>>().Object,
                null, null, null, null);
            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<UserApp>>();
            _signInManagerMock = new Mock<SignInManager<UserApp>>(_userManagerMock.Object,
                                                                  contextAccessor.Object,
                                                                  userPrincipalFactory.Object,
                                                                  null, null, null, null);

            _configurationMock = new Mock<IConfiguration>();
            _emailServiceMock = new Mock<EmailService>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _loggerMock = new Mock<ILogger<AccountController>>();

            _dbContext = new ApplicationDbContext(options);
            _controller = new AccountController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _configurationMock.Object,
                _emailServiceMock.Object,
                _dbContext,_loggerMock.Object
                , _rolManagerMock.Object
            );
        }

        [Fact]
        public async Task Register_UserCreated_ReturnsOk()
        {
            // Arrange
            var model = new AccountController.RegisterModel
            {
                Email = "test@example.com",
                Password = "Test@123"
            };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<UserApp>(), model.Password))
                            .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Register(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var user = new UserApp { Email = "test@example.com", UserName = "test@example.com" };

            var model = new AccountController.LoginModel
            {
                Email = "test@example.com",
                Password = "password"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email))
                            .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, model.Password))
                            .ReturnsAsync(true);

            _configurationMock.Setup(x => x["Jwt:Key"]).Returns("supersecretkey12345678901234567890");
            _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns("issuer");
            _configurationMock.Setup(x => x["Jwt:Audience"]).Returns("audience");

            // Act
            var result = await _controller.Login(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Token", ((dynamic)okResult.Value).ToString());
        }

        [Fact]
        public async Task ForgotPassword_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            string email = "unknown@example.com";
            _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync((UserApp)null);

            // Act
            var result = await _controller.ForgotPassword(email);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);
        }
    }
}
