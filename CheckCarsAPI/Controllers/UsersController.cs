using CheckCarsAPI.Models;
using CheckCarsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace CheckCarsAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        #region Properties
        private readonly EmailService _emailService;
        private readonly UserManager<UserApp> _userManager;
        private readonly SignInManager<UserApp> _signInManager;
        private readonly IConfiguration _configuration;
        #endregion

        public UsersController(
            UserManager<UserApp> userM,
            SignInManager<UserApp> SignInM,
            IConfiguration iconfig,
            EmailService serviceemail
        )
        {
            _emailService = serviceemail;
            _userManager = userM;
            _signInManager = SignInM;
            _configuration = iconfig;
        }

        #region Endpoints

        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _userManager.Users.ToList();
            users.ForEach(U => U.PasswordHash = string.Empty);
            return Ok(users);
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
        public async Task<IActionResult> AddUser([FromBody] UserApp user)
        {
            if (user == null)
            {
                return BadRequest("Invalid user data");
            }

            var result = await _userManager.CreateAsync(user);

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

        [HttpDelete("{id}")]
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
    }
}
