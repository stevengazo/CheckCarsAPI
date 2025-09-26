using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using CheckCarsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace CheckCarsAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        #region Properties
        private readonly EmailService _emailService;
        private readonly UserManager<UserApp> _userManager;
        private readonly SignInManager<UserApp> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _DbContext;
        #endregion

        #region Constructor
        public UsersController(
            UserManager<UserApp> userM,
            SignInManager<UserApp> SignInM,
            IConfiguration iconfig,
            EmailService serviceemail,
            ApplicationDbContext applicationDbContext

        )
        {
            _emailService = serviceemail;
            _userManager = userM;
            _signInManager = SignInM;
            _configuration = iconfig;
            _DbContext = applicationDbContext;
        }

        #endregion

        #region Endpoints

        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _userManager.Users.ToList();
            users.ForEach(U => U.PasswordHash = string.Empty);
            return Ok(users);
        }

        [HttpGet("/api/GetUsersDic")]
        public IActionResult GetUsersDic()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic = _userManager.Users.ToDictionary(U => U.Id, U => U.UserName);
            return Ok(dic);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }
            user.PasswordHash = string.Empty;
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] UserModel user)
        {
            if (user == null)
            {
                return BadRequest("Invalid user data");
            }

            var exists = _DbContext.Users.Where(e => e.Email == user.Email || e.UserName == user.UserName).Any();
            if (exists)
            {
                return BadRequest("User with this email or username already exists");
            }

            UserApp newUser = new UserApp
            {
                Email = user.Email,
                UserName = user.UserName,
                PasswordHash = _userManager.PasswordHasher.HashPassword(null, user.Password),
                EmailConfirmed = true // Confirm email by default
            };

            var result = await _userManager.CreateAsync(newUser);

            var userN = await _userManager.FindByEmailAsync(newUser.Email);
            var roleResult = await _userManager.AddToRoleAsync(userN, "User");

            if (result.Succeeded)
            {
                return Ok(user);
            }

            return BadRequest(result.Errors);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserApp updatedUser)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.PasswordHash = "";

            user.Email = updatedUser.Email;
            user.UserName = updatedUser.UserName;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok(user);
            }

            return BadRequest(result.Errors);
        }


        [HttpPatch("{id}")]
        public async Task<IActionResult> ConfirmUser(string id)
        {
            var User = await _userManager.FindByIdAsync(id);
            if (User == null)
            {
                return NotFound();
            }

            User.EmailConfirmed = !User.EmailConfirmed;

            _DbContext.Users.Update(User);
            _DbContext.SaveChanges();


            return NoContent();
        }

        [HttpPatch("password")]
        public async Task<IActionResult> PasswordUpdate([FromBody] UserPassword userPassword)
        {
            var user = await _userManager.FindByEmailAsync(userPassword.Email);
            if (user == null)
            {
                return NotFound();
            }

            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
            {
                return BadRequest(removeResult.Errors);
            }

            var addResult = await _userManager.AddPasswordAsync(user, userPassword.Password);
            if (!addResult.Succeeded)
            {
                return BadRequest(addResult.Errors);
            }

            return NoContent();
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return Ok("User deleted successfully");
            }

            return BadRequest(result.Errors);
        }

        #endregion


        public class UserPassword
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
        public class UserModel
        {
            public string Email { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
        }
    }
}
